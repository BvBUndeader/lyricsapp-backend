﻿using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LyricsBackend.Models
{
    [Table("favorites")]
    public class Favorites : BaseModel  
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("song_id")]
        public long SongId { get; set; }

        [Column("added_at")]
        public DateTime AddedAt { get; set; }

    }
}
