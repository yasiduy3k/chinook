namespace Chinook.ClientModels;

public class PlaylistCM
{
    public long PlaylistId { get; set; }
    public string Name { get; set; }
    public List<PlaylistTrackCM> Tracks { get; set; }
}