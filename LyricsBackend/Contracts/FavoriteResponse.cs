namespace LyricsBackend.Contracts
{
    public class FavoriteResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SongId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
