namespace LyricsBackend.Contracts
{
    public class AlbumResponse
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public DateOnly ReleaseDate { get; set; }
    }
}
