using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("lyrics")]
    public class Lyrics : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("song_id")]
        public long SongId { get; set; }

        [Column("language_code")]
        public string LanguageCode { get; set; }

        [Column("text")]
        public string Text { get; set; }

        public Songs Song { get; set; }
    }
}
