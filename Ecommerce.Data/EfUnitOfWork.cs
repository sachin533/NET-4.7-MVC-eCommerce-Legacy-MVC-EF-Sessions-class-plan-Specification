using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    /// <summary>
    /// Unit of Work over the shared per-request EcommerceDbContext.
    /// Repositories stage changes; services call Complete() once per operation.
    /// </summary>
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly EcommerceDbContext _context;

        public EfUnitOfWork(EcommerceDbContext context)
        {
            _context = context;
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }
    }
}
