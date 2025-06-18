using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("artists")]
    public class Artists : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("bio")]
        public string Bio {  get; set; }
    }
}
