﻿using SongAppApi.Models.Accounts;

namespace SongAppApi.Models.Songs
{
    public class SongResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public int Upvotes { get; set; }
        public DateTime CreatedAt { get; set; }
        //public int CreatedById { get; set; }
        public AccountResponse CreatedBy { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? SoundUrl { get; set; }
    }
}
