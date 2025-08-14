using Refit;
using TestWarehouse.Application.DTO;

namespace TestWarehouse.Web.Client.Services;

public interface IInventoryController
{
    [Post("/api/inventory/receipts")]
    Task<ApiResponse<Guid>> CreateReceipt([Body] ReceiptEditDto dto);

    [Put("/api/inventory/receipts/{id}")]
    Task<ApiResponse<Guid>> EditReceipt(Guid id, [Body] ReceiptEditDto dto);

    [Post("/api/inventory/shipments")]
    Task<ApiResponse<Guid>> CreateShipment([Body] ShipmentEditDto dto);

    [Post("/api/inventory/shipments/{id}/sign")]
    Task<ApiResponse<Guid>> SignShipment(Guid id);

    [Post("/api/inventory/shipments/{id}/unsign")]
    Task<ApiResponse<Guid>> UnsignShipment(Guid id);
}