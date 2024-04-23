namespace Chinook.IServices
{
    public interface IPlaylistService
    {
        Task<ClientModels.Playlist> GetPlaylist(long playlistId, string userId);
        Task<string> RemoveTrack(long playlistId, long trackId);
        Task<List<Models.Playlist>> GetPlaylistsOfUser(string userId);
        Task<string> AddTrackToPlayList(long trackId, long playlistId, string playlistName, string userId);
        Task DeletePlayList(long playlistId);
    }
}
