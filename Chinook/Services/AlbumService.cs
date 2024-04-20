using Chinook.IServices;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class AlbumService : IAlbumService
    {
        private IDBContextService dbContextService { get; set; }
        private ILogger<AlbumService> logger { get; set; }
        public AlbumService(ILogger<AlbumService> logger, IDBContextService dbContextService)
        {
            this.dbContextService = dbContextService;
            this.logger = logger;
        }

        /// <summary>
        /// Method for fetching the albums
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        // ----- Was not able to test ---------
        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetAlbumsForArtist - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }


    }
}
