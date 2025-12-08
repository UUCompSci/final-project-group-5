using Microsoft.EntityFrameworkCore;
using Entities;
using CellWorld;

namespace DataModel
{
    public class WorldContext : DbContext
    {
        public DbSet<LivingThing> Cells { get; set; }
        public DbSet<Cluster> Clusters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=world.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LivingThing>()
                .HasDiscriminator<string>("Type")
                .HasValue<Herbivore>("Herbivore")
                .HasValue<Carnivore>("Carnivore")
                .HasValue<Omnivore>("Omnivore")
                .HasValue<Alderbrook>("Alderbrook")
                .HasValue<AlienDidi>("AlienDidi");

            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Cells);
        }
    }
}
