using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class PlanetContext : DbContext
    {

        public DbSet<Server> Servers { get; set; }
        public DbSet<Trello> Trellos { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=localhost;user=root;database=planet;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(8, 0, 11)));
        }
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }

    public class Trello
    {
        public ulong Id { get; set; } // db ref only
        public ulong UserId { get; set; }
        public string Token { get; set; }
        public string BoardId { get; set; }
    }
}
