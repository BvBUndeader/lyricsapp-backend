namespace LyricsBackend.Contracts
{
    public class CreateSongHistoryResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SongId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
