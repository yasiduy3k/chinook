using Chinook.ClientModels;
using Chinook.IServices;
using Chinook.Models;
using Chinook.Statics;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        private IDBContextService dbContextService { get; set; }
        private ILogger<TrackService> logger { get; set; }

        public TrackService(ILogger<TrackService> logger, IDBContextService dBContextService)
        {
            this.dbContextService = dBContextService;
            this.logger = logger;

        }
        /// <summary>
        /// Get all playlists and tracks
        /// </summary>
        /// <param name="artistId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<PlaylistTrack>> GetPlaylistTracks(long artistId, string currentUserId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var playlistTracks = dbContext.Tracks
                .Include(a => a.Album)
                .Where(a => a.Album!=null && a.Album.ArtistId == artistId)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == Constants.FavouritePlayListName)).Any()
                })
                .OrderBy(a => a.AlbumTitle) //Lets order for easy reference
                    .ThenBy(a=>a.TrackName)
                .ToList();
                return playlistTracks;
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetPlaylistTracks - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }
        /// <summary>
        /// Add to my favourite playlist
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> AddToMyFavourite(long trackId, string userId)
        {
            var message = string.Empty;
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var track = dbContext.Tracks.SingleOrDefault(t => t.TrackId == trackId);
                if (track == null)
                {
                    message = " is not existing.";
                }
                else
                {
                    var favPlaylist = dbContext.Playlists
                        .Include(p => p.UserPlaylists)
                        .Include(p => p.Tracks)
                        .Where(p => p.Name == Constants.FavouritePlayListName && p.UserPlaylists.Any(x => x.UserId == userId))
                        .FirstOrDefault();
                    //--- If first time/not existing we need to create -----
                    if (favPlaylist == null)
                    {
                        long playlistId = 1;
                        if(dbContext.Playlists.Any())
                        {
                            playlistId = dbContext.Playlists.Max(p => p.PlaylistId) + 1;
                        }
                        favPlaylist = new Models.Playlist()
                        {
                            Name = Constants.FavouritePlayListName,
                            Tracks = new List<Track>(),
                            UserPlaylists = new List<UserPlaylist>(),
                            PlaylistId= playlistId
                        };
                        favPlaylist.UserPlaylists.Add(new UserPlaylist() { Playlist = favPlaylist, UserId = userId });
                        favPlaylist.Tracks.Add(track);
                        dbContext.Playlists.Add(favPlaylist);
                        dbContext.SaveChanges();
                        message = $" added to playlist {Constants.FavouritePlayListName}.";
                    }
                    else
                    {
                        var isTrackExists = favPlaylist.Tracks.Any(t => t.TrackId == trackId);
                        if (isTrackExists)
                        {
                            message = $" is already in {Constants.FavouritePlayListName}.";
                        }
                        else
                        {
                            favPlaylist.Tracks.Add(track);
                            dbContext.Update(favPlaylist);
                            dbContext.SaveChanges();
                            message = $" added to playlist {Constants.FavouritePlayListName}.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: AddToMyFavourite - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                message = $" could not be added to {Constants.FavouritePlayListName}. Please try again in a few minutes!";
            }

            return message;
        }

        /// <summary>
        /// Remove from my favourite platlist
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> RemoveFromMyFavourite(long trackId, string userId)
        {
            var message = $" was not in the playlist {Constants.FavouritePlayListName}.";

            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var favPlaylist = dbContext.Playlists
                            .Include(p => p.UserPlaylists)
                            .Include(p => p.Tracks)
                            .Where(p => p.Name == Constants.FavouritePlayListName && p.UserPlaylists.Any(x => x.UserId == userId) && p.Tracks.Any(t => t.TrackId == trackId))
                            .FirstOrDefault();
                // ---- Can remove only if it exists in the playlist ------
                if (favPlaylist != null)
                {
                    var track = dbContext.Tracks.SingleOrDefault(t => t.TrackId == trackId);
                    if (track != null)
                    {
                        favPlaylist.Tracks.Remove(track);
                        dbContext.Update(favPlaylist);
                        dbContext.SaveChanges();
                        message = $" removed from playlist {Constants.FavouritePlayListName}.";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: RemoveFromMyFavourite - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                message = $" could not be removed from {Constants.FavouritePlayListName}. Please try again in a few minutes!";
            }

            return message;
        }

    }
}
