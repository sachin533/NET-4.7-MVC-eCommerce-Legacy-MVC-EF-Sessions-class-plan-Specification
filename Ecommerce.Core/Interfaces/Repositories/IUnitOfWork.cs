namespace Ecommerce.Core.Interfaces.Repositories
{
    /// <summary>
    /// Unit of Work: repositories stage changes on the shared per-request DbContext,
    /// the service calls Complete() once to commit them (single SaveChanges).
    /// </summary>
    public interface IUnitOfWork
    {
        int Complete();
    }
}
