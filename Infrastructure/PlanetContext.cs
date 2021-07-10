using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class PlanetContext : DbContext
    {

        public DbSet<Server> Servers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<TrelloTokens> TrelloTokens { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=localhost;user=root;database=planet;port=3306;Connect Timeout=5;", new MySqlServerVersion(new Version(8, 0, 11)));
        }
    }
    public class Server
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public string TrelloKey { get; set; }
    }

    public class Rank
    {
        public int Id { get; set; } // db ref only
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }

    public class TrelloTokens
    {
        public int Id { get; set; } // db ref only
        public ulong UserId { get; set; }
        public string TrelloToken { get; set; }
    }
}
