using Chinook.ClientModels;
using Chinook.IServices;
using Chinook.Models;
using Chinook.Statics;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class PlaylistService : IPlaylistService
    {
        private IDBContextService dbContextService { get; set; }
        private ILogger<PlaylistService> logger { get; set; }
        public PlaylistService(ILogger<PlaylistService> logger, IDBContextService dbContextService)
        {
            this.dbContextService = dbContextService;
            this.logger = logger;
        }

        /// <summary>
        /// Method for fetching the playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ClientModels.Playlist> GetPlaylist(long playlistId, string userId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var playlist = dbContext.Playlists
                    .Include(a => a.Tracks)
                    .ThenInclude(a => a.Album)
                    .ThenInclude(a => a.Artist)
                    .Where(p => p.PlaylistId == playlistId)
               .Select(p => new ClientModels.Playlist()
               {
                   Name = p.Name == null ? "" : p.Name,
                   Tracks = p.Tracks.Select(t => new PlaylistTrack()
                   {
                       AlbumTitle = t.Album == null ? "" : t.Album.Title,
                       ArtistName = t.Album == null ? "" : t.Album.Artist == null ? "" : t.Album.Artist.Name == null ? "" : t.Album.Artist.Name,
                       TrackId = t.TrackId,
                       TrackName = t.Name,
                       IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == Constants.FavouritePlayListName)).Any()
                   })
                   .OrderBy(t => t.ArtistName) //Lets order for easy reference
                    .ThenBy(t => t.AlbumTitle)
                    .ThenBy(t => t.TrackName)
                   .ToList()
               })
               .FirstOrDefault();
                return playlist;
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetPlaylist - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }

        /// <summary>
        /// Remove a track from the playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public async Task<string> RemoveTrack(long playlistId, long trackId)
        {
            var message = string.Empty;
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var playlist = dbContext.Playlists
                    .Include(p => p.Tracks)
                    .SingleOrDefault(p => p.PlaylistId == playlistId);
                //---------- Check if playlist is existing -----------
                if (playlist == null)
                {
                    message = "playlist does not exists!";
                }
                else
                {
                    //-------- Check if track is in the playlist ------
                    var track = playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                    if (track == null)
                    {
                        message = "track does not exists in playlist";
                    }
                    else
                    {
                        playlist.Tracks.Remove(track);
                        dbContext.Playlists.Update(playlist);
                        dbContext.SaveChanges();
                        message = " successfully removed.";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: RemoveTrack - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                message = " could not be removed. Please try again in a few minutes!";
            }

            return message;
        }

        /// <summary>
        /// Load play lists for the user modal
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //--- This should be refactored to use name value pair (used for drop down) to make t light weight ------
        public async Task<List<Models.Playlist>> GetPlaylistsOfUser(string userId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var playlists = dbContext.Playlists
                    .Include(p => p.UserPlaylists)
                    .Where(p => p.UserPlaylists.Any(up => up.UserId == userId))
                    .OrderByDescending(p=>p.Name==Constants.FavouritePlayListName) //Order to keep fav playlist always as the top
                    .ThenBy(p => p.Name)
                    .ToList();
                return playlists;
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: GetPlaylistsOfUser - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                throw new Exception("Server Error! Please retry in a few moments");
            }
        }

        /// <summary>
        /// Adding track to play list
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="playlistId"></param>
        /// <param name="playlistName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        //--- Giving prominancy no new playlist name used for the moment ----
        //--- Ideally UI should be restricted to handle this --------
        public async Task<string> AddTrackToPlayList(long trackId, long playlistId, string playlistName, string userId)
        {
            var message = " not added!";
            bool isExistingPlaylist = true;
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var track = dbContext.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                //---- If the track has been deleted while deciding on a new/existing playlist by another then cannot proceed ----
                if (track == null)
                {
                    message = " not addedd since track is not available!";
                }
                else
                {
                    Models.Playlist playlist = null;
                    //--- If a new name is not given then assign to the existing id ------
                    if (string.IsNullOrWhiteSpace(playlistName))
                    {
                        playlist = dbContext.Playlists
                            .Include(p => p.Tracks)
                            .Include(p => p.UserPlaylists)
                            .FirstOrDefault(pl => pl.PlaylistId == playlistId && pl.UserPlaylists != null && pl.UserPlaylists.Any(up => up.UserId == userId));
                    }
                    else
                    {
                        //------ If name is given first check if it's an existing play list --------
                        playlist = dbContext.Playlists
                            .Include(p => p.Tracks)
                            .Include(p => p.UserPlaylists)
                            .FirstOrDefault(pl => pl.Name.ToLower() == playlistName.ToLower() && pl.UserPlaylists.Any(up => up.UserId == userId));
                    }
                    //------ If new name is given and it has no relevant record we add a new playlist --------------
                    if (playlist == null && !string.IsNullOrWhiteSpace(playlistName))
                    {
                        isExistingPlaylist = false;
                        long newId = 1;
                        if (dbContext.Playlists.Any())
                        {
                            newId = dbContext.Playlists.Max(p => p.PlaylistId) + 1;
                        }

                        playlist = new Models.Playlist()
                        {
                            Name = playlistName,
                            Tracks = new List<Track>(),
                            UserPlaylists = new List<UserPlaylist>(),
                            PlaylistId = newId
                        };
                        playlist.UserPlaylists.Add(new UserPlaylist() { UserId = userId });
                    }

                    if (playlist == null)
                    {
                        message = " not added since selected playlist is not existing and a new playlist name has not been provided!";
                    }
                    else if (playlist.Tracks.Any(t => t.TrackId == trackId))
                    {
                        message = $" not added since track is already existing in playist {playlist.Name}";
                    }
                    else
                    {
                        playlist.Tracks.Add(track);
                        if (isExistingPlaylist)
                        {
                            dbContext.Playlists.Update(playlist);
                        }
                        else
                        {
                            dbContext.Playlists.Add(playlist);
                        }
                        dbContext.SaveChanges();
                        message = $" sucessfully added to playlist {playlist.Name}";
                    }
                }
                return message;
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: AddTrackToPlayList - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
                message = $" was terminated. Please try again in a few minutes.";
            }

            return message;
        }

        /// <summary>
        /// Delete a playlist completely
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        public async Task DeletePlayList(long playlistId)
        {
            try
            {
                var dbContext = await dbContextService.GetContextAsync();
                var playlist = dbContext.Playlists
                              .Include(p => p.Tracks)
                              .Include(p => p.UserPlaylists)
                              .FirstOrDefault(p => p.PlaylistId == playlistId);

                if (playlist != null)
                {
                    //Remove tracks
                    foreach (var item in playlist.Tracks)
                    {
                        playlist.Tracks.Remove(item);
                    }
                    //Remove from user playlists
                    foreach (var item in playlist.UserPlaylists)
                    {
                        playlist.UserPlaylists.Remove(item);
                    }
                    //Remove from playlists
                    dbContext.Playlists.Remove(playlist);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Method: DeletePlayList - Inner: {ex.InnerException} - Message: {ex.Message} - Stack: {ex.StackTrace}", ex);
            }
        }
    }
}
