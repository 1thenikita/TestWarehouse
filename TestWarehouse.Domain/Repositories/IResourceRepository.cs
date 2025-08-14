using TestWarehouse.Domain.Entities;
using TestWarehouse.Domain.Interfaces;

namespace TestWarehouse.Domain.Repositories;

public interface IResourceRepository
{
    
}

public interface IBalanceRepository : IRepository<Balance, Guid>
{
    Task<Balance?> GetByResourceAndUnitLocked(Guid resourceId, Guid unitId, CancellationToken ct = default);
}

public interface IDocumentRepository : IRepository<Document, Guid>
{
    Task<bool> AnyInUnSignedDocuments(Guid resourceId, CancellationToken ct = default);
}