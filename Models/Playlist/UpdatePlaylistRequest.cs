namespace SongAppApi.Models.Playlist
{
    public class UpdatePlaylistRequest
    {
        public string Name { get; set; }
        public List<int> SongIds { get; set; }
    }
}
