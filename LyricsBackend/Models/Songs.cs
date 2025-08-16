using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("songs")]
    public class Songs : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("album_id")]
        public long AlbumId { get; set; }

        [Column("artist_id")]
        public long ArtistId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("lyrics")]
        public string Lyrics { get; set; }

        //[Column("album")]
        public Albums Album { get; set; }

        //[Column("artist")]
        public Artists Artist { get; set; }
    }
}
