namespace LyricsBackend.Contracts
{
    public class FavoriteFetchResponse
    {
        public long SongId { get; set; }
        public string SongTitle { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
    }
}
