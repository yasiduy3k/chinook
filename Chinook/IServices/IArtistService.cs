using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.IServices
{
    public interface IArtistService
    {
        Task<List<Artist>> GetArtists(string searchFilter);
        Task<Artist> GetArtistById(long artistId);
    }
}
