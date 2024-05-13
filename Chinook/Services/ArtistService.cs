using Chinook.ClientModels;
using Chinook.IServices;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private IDBContextService dbContextService { get; set; }
        private ILogger<AlbumService> logger { get; set; }
        public ArtistService(ILogger<AlbumService> logger, IDBContextService dbContextService)
        {
            this.dbContextService = dbContextService;
            this.logger = logger;
        }
        /// <summary>
        /// Fetch artists based on search filter
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ArtistCM>> GetArtists(string searchFilter)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                //----- If searchFilter has no value fetch all else filter -----
                if (string.IsNullOrWhiteSpace(searchFilter))
                {
                    return dbContext.Artists.Include(a => a.Albums)
                        .Select(a => new ArtistCM { ArtistId = a.ArtistId, Name = a.Name, AlbumCount = a.Albums.Count })
                        .OrderBy(a => a.Name)
                        .ToList();
                }
                else
                {
                    return dbContext.Artists
                        .Include(a => a.Albums)
                        .Where(a => a.Name != null && a.Name.ToLower().Contains(searchFilter.ToLower()))
                        .Select(a => new ArtistCM { ArtistId = a.ArtistId, Name = a.Name, AlbumCount = a.Albums.Count })
                        .OrderBy(a => a.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetArtists - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }

        /// <summary>
        /// Method for fetching a particular artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ArtistCM> GetArtistById(long artistId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var artist = dbContext.Artists
                    .Where(a => a.ArtistId == artistId)
                    .Select(a => new ArtistCM { ArtistId = a.ArtistId, Name = a.Name })
                    .SingleOrDefault();
                return artist;
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetArtistById - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }
    }
}
