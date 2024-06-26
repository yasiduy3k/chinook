﻿using Chinook.ClientModels;

namespace Chinook.IServices
{
    public interface ITrackService
    {
        Task<List<PlaylistTrackCM>> GetPlaylistTracks(long artistId, string currentUserId);
        Task<string> AddToMyFavourite(long trackId, string userId);
        Task<string> RemoveFromMyFavourite(long trackId, string userId);
    }
}
