using TestWarehouse.Domain.Enums;

namespace TestWarehouse.Application.DTO;

public record ResourceDto(Guid ID, string Name, bool IsArchived);
public record ResourceEditDto(string Name, bool IsArchived);

public record UnitDto(Guid ID, string Name, bool IsArchived);
public record UnitEditDto(string Name, bool IsArchived);

public record BalanceDto(Guid ID, Guid ResourceId, string ResourceName, Guid UnitId, string UnitName, decimal Quantity);

public record DocumentItemDto(Guid ResourceId, Guid UnitId, decimal Quantity, string? ResourceName = null, string? UnitName = null);

public record ReceiptDto(Guid ID, string Number, DateTime Date, DocumentState State, List<DocumentItemDto> Items);
public record ReceiptEditDto(string Number, DateTime Date, List<DocumentItemDto> Items);

public record ShipmentDto(Guid ID, string Number, DateTime Date, DocumentState State, List<DocumentItemDto> Items);
public record ShipmentEditDto(string Number, DateTime Date, List<DocumentItemDto> Items);

