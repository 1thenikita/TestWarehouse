using TestWarehouse.Domain.Enums;

namespace TestWarehouse.Domain.Entities;

public class Resource
{
    public Guid ID { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }

    public ICollection<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
    public ICollection<Balance> Balances { get; set; } = new List<Balance>();
}

public class Unit
{
    public Guid ID { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }

    public ICollection<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
    public ICollection<Balance> Balances { get; set; } = new List<Balance>();
}

public class Balance
{
    public Guid ID { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }

    public Resource Resource { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
}

public abstract class Document
{
    public Guid ID { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public DocumentState State { get; set; } = DocumentState.Draft;

    public ICollection<DocumentItem> Items { get; set; } = new List<DocumentItem>();
}

public class ShipmentDocument : Document
{
}

public class DocumentItem
{
    public Guid ID { get; set; }
    public Guid DocumentId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }

    public Document Document { get; set; } = null!;
    public Resource Resource { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
}


public class ReceiptDocument : Document
{
}