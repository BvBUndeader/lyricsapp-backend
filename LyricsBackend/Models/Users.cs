using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("users")]
    public class Users : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
