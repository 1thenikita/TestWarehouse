using Microsoft.EntityFrameworkCore;
using TestWarehouse.Domain.Entities;
using TestWarehouse.Domain.Enums;
using TestWarehouse.Domain.Interfaces;
using TestWarehouse.Domain.Repositories;
using TestWarehouse.Infrastructure.Persistence;

namespace TestWarehouse.Infrastructure.Repositories;

public class ResourceRepository : Repository<Resource, Guid>, IResourceRepository
{
    private readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _db = db;
    }

    public override DbSet<Resource> GetDbSet() => _db.Resources;
    public override DbContext GetDbContext() => _db;
}

public class UnitRepository : Repository<Unit, Guid>
{
    private readonly AppDbContext _db;

    public UnitRepository(AppDbContext db)
    {
        _db = db;
    }

    public override DbSet<Unit> GetDbSet() => _db.Units;

    public override DbContext GetDbContext() => _db;
}

public class BalanceRepository : Repository<Balance, Guid>, IBalanceRepository
{
    private readonly AppDbContext _db;

    public BalanceRepository(AppDbContext db)
    {
        _db = db;
    }

    public override DbSet<Balance> GetDbSet() => _db.Balances;
    public override DbContext GetDbContext() => _db;

    /// <summary>
    /// Получает баланс по ресурсу и единице с блокировкой строки (SELECT FOR UPDATE)
    /// </summary>
    public async Task<Balance?> GetByResourceAndUnitLocked(Guid resourceId, Guid unitId, CancellationToken ct = default)
    {
        return await _db.Balances
            .FromSql($@"
                    SELECT * FROM ""Balances""
                    WHERE ""ResourceId"" = {resourceId} AND ""UnitId"" = {unitId}
                    FOR UPDATE")
            .FirstOrDefaultAsync(ct);
    }
}

public class DocumentRepository : Repository<Document, Guid>, IDocumentRepository
{
    private readonly AppDbContext _db;

    public DocumentRepository(AppDbContext db)
    {
        _db = db;
    }

    public override DbSet<Document> GetDbSet() => _db.Documents;
    public override DbContext GetDbContext() => _db;

    /// <summary>
    /// Проверяет, используется ли ресурс в неподписанных документах
    /// </summary>
    public async Task<bool> AnyInUnSignedDocuments(Guid resourceId, CancellationToken ct = default)
    {
        return await _db.Documents
            .Include(d => d.Items)
            .AnyAsync(d =>
                d.State != DocumentState.Signed &&
                d.Items.Any(i => i.ResourceId == resourceId), ct);
    }
}