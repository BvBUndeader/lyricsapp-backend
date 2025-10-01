namespace LyricsBackend.Contracts
{
    public class CreateSongHistoryRequest
    {
        public long UserId { get; set; }
        public long SongId { get; set; }
    }
}
