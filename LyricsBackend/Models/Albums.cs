using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("albums")]
    public class Albums : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("artist_id")]
        public long ArtistId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("genre")]
        public string Genre { get; set; }

        [Column("release_date")]
        public DateOnly ReleaseDate { get; set; }
    }
}
