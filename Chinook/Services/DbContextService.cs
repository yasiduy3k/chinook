using Chinook.IServices;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    //---- To handle the context creation from a single place ----
    public class DbContextService : IDBContextService
    {
        private IDbContextFactory<ChinookContext> DbFactory { get; set; }

        public DbContextService(IDbContextFactory<ChinookContext> DbFactory)
        {
            this.DbFactory = DbFactory;
        }
        /// <summary>
        /// For creating and sharing the context
        /// </summary>
        /// <returns></returns>
        public async Task<ChinookContext> GetContextAsync()
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext;
        }
    }
}
