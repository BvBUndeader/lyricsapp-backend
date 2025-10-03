using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("opened_history")]
    public class OpenedHistory : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }
       
        [Column("user_id")]
        public long UserId { get; set; }
        [Column("song_id")]
        public long SongId { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
