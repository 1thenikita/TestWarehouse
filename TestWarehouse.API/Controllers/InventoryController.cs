using Microsoft.AspNetCore.Mvc;
using TestWarehouse.Application.DTO;
using TestWarehouse.Application.Results;
using TestWarehouse.Application.Services;

namespace TestWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // ---------- Поступления ----------
    [HttpPost("receipts")]
    public async Task<ActionResult> CreateReceipt([FromBody] ReceiptEditDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.CreateReceiptAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("receipts/{id}")]
    public async Task<ActionResult> EditReceipt(Guid id, [FromBody] ReceiptEditDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.EditReceiptAsync(id, dto, ct);
        return result.ToActionResult();
    }

    // ---------- Отгрузки ----------
    [HttpPost("shipments")]
    public async Task<ActionResult> CreateShipment([FromBody] ShipmentEditDto dto, CancellationToken ct)
    {
        var result = await _inventoryService.CreateShipmentAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPost("shipments/{id}/sign")]
    public async Task<ActionResult> SignShipment(Guid id, CancellationToken ct)
    {
        var result = await _inventoryService.SignShipmentAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost("shipments/{id}/unsign")]
    public async Task<ActionResult> UnsignShipment(Guid id, CancellationToken ct)
    {
        var result = await _inventoryService.UnsignShipmentAsync(id, ct);
        return result.ToActionResult();
    }
}