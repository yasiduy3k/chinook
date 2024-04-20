using Chinook.Models;

namespace Chinook.IServices
{
    public interface IAlbumService
    {
        Task<List<Album>> GetAlbumsForArtist(int artistId);
    }
}
