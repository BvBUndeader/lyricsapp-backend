using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("artist_bio")]
    public class ArtistBio : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("artist_id")]
        public long ArtistId { get; set; }

        [Column("language_code")]
        public string LanguageCode { get; set; }

        [Column("text")]
        public string Text { get; set; }

        public Artists Artist { get; set; }
    }
}
