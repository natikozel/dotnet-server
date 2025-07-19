using Microsoft.EntityFrameworkCore;
using Connect4Server.Models;

namespace Connect4Server.Data
{
    public class Connect4Context : DbContext
    {
        public Connect4Context(DbContextOptions<Connect4Context> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = default!;
        public DbSet<Game> Games { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.PlayerId)
                .IsUnique();

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player)
                .WithMany()
                .HasForeignKey(g => g.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
} 