namespace LyricsBackend.Contracts
{
    public class PassChangeRequest
    {
        public long UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
