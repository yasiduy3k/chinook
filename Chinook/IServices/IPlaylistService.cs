using Chinook.ClientModels;

namespace Chinook.IServices
{
    public interface IPlaylistService
    {
        Task<PlaylistCM> GetPlaylist(long playlistId, string userId);
        Task<string> RemoveTrack(long playlistId, long trackId);
        Task<List<PlaylistCM>> GetPlaylistsOfUser(string userId);
        Task<string> AddTrackToPlayList(long trackId, long playlistId, string playlistName, string userId);
        Task DeletePlayList(long playlistId);
    }
}
