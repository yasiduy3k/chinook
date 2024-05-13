using Chinook.IServices;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    //---- To handle the context creation from a single place ----
    public class DbContextService : IDBContextService
    {
        private IDbContextFactory<ChinookContext> dbFactory { get; set; }

        public DbContextService(IDbContextFactory<ChinookContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
        /// <summary>
        /// For creating and sharing the context
        /// </summary>
        /// <returns></returns>
        public async Task<ChinookContext> GetContextAsync()
        {
            var dbContext = await dbFactory.CreateDbContextAsync();
            return dbContext;
        }
    }
}
