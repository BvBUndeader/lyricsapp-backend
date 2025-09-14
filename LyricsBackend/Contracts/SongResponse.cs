namespace LyricsBackend.Contracts
{
    public class SongResponse
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Lyrics { get; set; }
    }
}
