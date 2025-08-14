using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestWarehouse.Application.DTO;
using TestWarehouse.Application.Results;
using TestWarehouse.Domain.Entities;
using TestWarehouse.Domain.Enums;
using TestWarehouse.Domain.Interfaces;
using TestWarehouse.Domain.Repositories;
using TestWarehouse.Infrastructure.Persistence;

namespace TestWarehouse.Application.Services;

public interface IInventoryService
{
    Task<Result<Guid>> CreateReceiptAsync(ReceiptEditDto dto, CancellationToken ct = default);
    Task<Result> EditReceiptAsync(Guid id, ReceiptEditDto dto, CancellationToken ct = default);

    Task<Result<Guid>> CreateShipmentAsync(ShipmentEditDto dto, CancellationToken ct = default);
    Task<Result> SignShipmentAsync(Guid id, CancellationToken ct = default);
    Task<Result> UnsignShipmentAsync(Guid id, CancellationToken ct = default);
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly IBalanceRepository _balanceRepo;
    private readonly IRepository<Resource, Guid> _resourceRepo;
    private readonly IRepository<Unit, Guid> _unitRepo;
    private readonly IRepository<ReceiptDocument, Guid> _receiptRepo;
    private readonly IRepository<ShipmentDocument, Guid> _shipmentRepo;
    private readonly ILogger<InventoryService> _logger;
    private readonly IMapper _mapper;

    public InventoryService(
        AppDbContext db,
        IBalanceRepository balanceRepo,
        IRepository<Resource, Guid> resourceRepo,
        IRepository<Unit, Guid> unitRepo,
        IRepository<ReceiptDocument, Guid> receiptRepo,
        IRepository<ShipmentDocument, Guid> shipmentRepo,
        ILogger<InventoryService> logger,
        IMapper mapper)
    {
        _db = db;
        _balanceRepo = balanceRepo;
        _resourceRepo = resourceRepo;
        _unitRepo = unitRepo;
        _receiptRepo = receiptRepo;
        _shipmentRepo = shipmentRepo;
        _logger = logger;
        _mapper = mapper;
    }

    // ----------------- Поступление -----------------
    public async Task<Result<Guid>> CreateReceiptAsync(ReceiptEditDto dto, CancellationToken ct = default)
    {
        if (dto.Items.Any(i => i.Quantity <= 0))
            return Result<Guid>.Fail("Количество должно быть больше нуля");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var doc = _mapper.Map<ReceiptDocument>(dto);
            doc.ID = Guid.NewGuid();

            foreach (var item in dto.Items)
            {
                var resource = await _resourceRepo.GetById(item.ResourceId, ct);
                if (resource == null)
                    return Result<Guid>.Fail($"Ресурс {item.ResourceId} не найден");
                if (resource.IsArchived)
                    return Result<Guid>.Fail($"Ресурс {resource.Name} в архиве");

                var unit = await _unitRepo.GetById(item.UnitId, ct);
                if (unit == null)
                    return Result<Guid>.Fail($"Единица {item.UnitId} не найдена");

                var balance = await _balanceRepo.GetByResourceAndUnitLocked(item.ResourceId, item.UnitId, ct);
                if (balance == null)
                {
                    balance = new Balance
                    {
                        ID = Guid.NewGuid(),
                        ResourceId = item.ResourceId,
                        UnitId = item.UnitId,
                        Quantity = 0
                    };
                    _db.Balances.Add(balance);
                }
                balance.Quantity += item.Quantity;
            }

            _db.ReceiptDocuments.Add(doc);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result<Guid>.Ok(doc.ID);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Ошибка создания поступления");
            return Result<Guid>.Fail("Ошибка при создании документа");
        }
    }

    public async Task<Result> EditReceiptAsync(Guid id, ReceiptEditDto dto, CancellationToken ct = default)
    {
        var existing = await _db.ReceiptDocuments
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.ID == id, ct);

        if (existing == null)
            return Result.Fail("Документ не найден");

        if (existing.State == DocumentState.Signed)
            return Result.Fail("Нельзя редактировать подписанный документ");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // Откат остатков по старым позициям
            foreach (var oldItem in existing.Items)
            {
                var balance = await _balanceRepo.GetByResourceAndUnitLocked(oldItem.ResourceId, oldItem.UnitId, ct);
                if (balance != null)
                    balance.Quantity -= oldItem.Quantity;
            }

            existing.Items.Clear();

            foreach (var newItem in dto.Items)
            {
                if (newItem.Quantity <= 0)
                    return Result.Fail("Количество должно быть > 0");

                var balance = await _balanceRepo.GetByResourceAndUnitLocked(newItem.ResourceId, newItem.UnitId, ct);
                if (balance == null)
                {
                    balance = new Balance
                    {
                        ID = Guid.NewGuid(),
                        ResourceId = newItem.ResourceId,
                        UnitId = newItem.UnitId,
                        Quantity = 0
                    };
                    _db.Balances.Add(balance);
                }
                balance.Quantity += newItem.Quantity;

                existing.Items.Add(_mapper.Map<DocumentItem>(newItem));
            }

            existing.Number = dto.Number;
            existing.Date = dto.Date;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Ошибка редактирования поступления");
            return Result.Fail("Ошибка при редактировании");
        }
    }

    // ----------------- Отгрузка -----------------
    public async Task<Result<Guid>> CreateShipmentAsync(ShipmentEditDto dto, CancellationToken ct = default)
    {
        if (dto.Items.Any(i => i.Quantity <= 0))
            return Result<Guid>.Fail("Количество должно быть > 0");

        var doc = _mapper.Map<ShipmentDocument>(dto);
        doc.ID = Guid.NewGuid();
        _db.ShipmentDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);

        return Result<Guid>.Ok(doc.ID);
    }

    public async Task<Result> SignShipmentAsync(Guid id, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var doc = await _db.ShipmentDocuments
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.ID == id, ct);

            if (doc == null)
                return Result.Fail("Документ не найден");

            if (!doc.Items.Any())
                return Result.Fail("Нельзя подписать пустой документ");

            foreach (var item in doc.Items)
            {
                var balance = await _balanceRepo.GetByResourceAndUnitLocked(item.ResourceId, item.UnitId, ct);
                if (balance == null || balance.Quantity < item.Quantity)
                    return Result.Fail($"Недостаточно ресурса {item.ResourceId} (есть {balance?.Quantity ?? 0}, нужно {item.Quantity})");
            }

            foreach (var item in doc.Items)
            {
                var balance = await _balanceRepo.GetByResourceAndUnitLocked(item.ResourceId, item.UnitId, ct);
                balance!.Quantity -= item.Quantity;
            }

            doc.State = DocumentState.Signed;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Ошибка подписания отгрузки");
            return Result.Fail("Ошибка при подписании");
        }
    }

    public async Task<Result> UnsignShipmentAsync(Guid id, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var doc = await _db.ShipmentDocuments
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.ID == id, ct);

            if (doc == null)
                return Result.Fail("Документ не найден");

            if (doc.State != DocumentState.Signed)
                return Result.Fail("Документ не подписан");

            foreach (var item in doc.Items)
            {
                var balance = await _balanceRepo.GetByResourceAndUnitLocked(item.ResourceId, item.UnitId, ct);
                if (balance == null)
                {
                    balance = new Balance
                    {
                        ID = Guid.NewGuid(),
                        ResourceId = item.ResourceId,
                        UnitId = item.UnitId,
                        Quantity = 0
                    };
                    _db.Balances.Add(balance);
                }
                balance.Quantity += item.Quantity;
            }

            doc.State = DocumentState.Draft;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Ошибка отмены подписания");
            return Result.Fail("Ошибка при отмене подписания");
        }
    }
}