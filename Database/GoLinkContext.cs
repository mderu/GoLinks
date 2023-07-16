using GoLinks.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace GoLinks.Database
{
    public class GoLinkContext : DbContext
    {
        public GoLinkContext(DbContextOptions<GoLinkContext> options) : base(options)
        {
        }

        public DbSet<GoLink> GoLinks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=GoLinks;Trusted_Connection=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoLink>().ToTable("GoLinks");
        }
    }
}
