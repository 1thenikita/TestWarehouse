using Microsoft.EntityFrameworkCore;
using TestWarehouse.Domain.Entities;

namespace TestWarehouse.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<ReceiptDocument> ReceiptDocuments => Set<ReceiptDocument>();

    public DbSet<ShipmentDocument> ShipmentDocuments => Set<ShipmentDocument>();
    public DbSet<Document> Documents => Set<Document>();
}