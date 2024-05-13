using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.IServices
{
    public interface IArtistService
    {
        Task<List<ArtistCM>> GetArtists(string searchFilter);
        Task<ArtistCM> GetArtistById(long artistId);
    }
}
