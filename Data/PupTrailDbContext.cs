using Microsoft.EntityFrameworkCore;
using PupTrailsV3.Models;
using System;
using System.IO;

namespace PupTrailsV3.Data
{
    public class PupTrailDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripAnimal> TripAnimals { get; set; }
        public DbSet<VetVisit> VetVisits { get; set; }
        public DbSet<VetService> VetServices { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<Intake> Intakes { get; set; }
        public DbSet<MoneyOwed> MoneyOwed { get; set; }
        public DbSet<PuppyGroup> PuppyGroups { get; set; }
        public DbSet<License> Licenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use shared AppData storage via PathManager to ensure consistency across builds
            var dataDir = PupTrailsV3.Services.PathManager.DataDirectory;
            var dbPath = Path.Combine(dataDir, "PupTrail.db");

            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Animals
            modelBuilder.Entity<Animal>()
                .HasIndex(a => a.Status);
            modelBuilder.Entity<Animal>()
                .HasIndex(a => a.IntakeDate);
            modelBuilder.Entity<Animal>()
                .HasMany(a => a.VetVisits)
                .WithOne(v => v.Animal)
                .HasForeignKey(v => v.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Animal>()
                .HasMany(a => a.Adoptions)
                .WithOne(ad => ad.Animal)
                .HasForeignKey(ad => ad.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Animal>()
                .HasMany(a => a.Expenses)
                .WithOne(e => e.Animal)
                .HasForeignKey(e => e.AnimalId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Animal>()
                .HasMany(a => a.Incomes)
                .WithOne(i => i.Animal)
                .HasForeignKey(i => i.AnimalId)
                .OnDelete(DeleteBehavior.SetNull);

            // Trips
            modelBuilder.Entity<Trip>()
                .HasIndex(t => t.Date);
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripAnimals)
                .WithOne(ta => ta.Trip)
                .HasForeignKey(ta => ta.TripId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Expenses)
                .WithOne(e => e.Trip)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            // TripAnimal
            modelBuilder.Entity<TripAnimal>()
                .HasKey(ta => new { ta.TripId, ta.AnimalId });
            modelBuilder.Entity<TripAnimal>()
                .HasOne(ta => ta.Animal)
                .WithMany(a => a.TripAnimals)
                .HasForeignKey(ta => ta.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);

            // VetVisits
            modelBuilder.Entity<VetVisit>()
                .HasMany(v => v.Services)
                .WithOne(s => s.VetVisit)
                .HasForeignKey(s => s.VetVisitId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<VetVisit>()
                .HasOne(v => v.Person)
                .WithMany(p => p.VetVisits)
                .HasForeignKey(v => v.PersonId)
                .OnDelete(DeleteBehavior.SetNull);

            // Adoptions
            modelBuilder.Entity<Adoption>()
                .HasOne(ad => ad.Person)
                .WithMany(p => p.Adoptions)
                .HasForeignKey(ad => ad.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Income
            modelBuilder.Entity<Income>()
                .HasOne(i => i.Person)
                .WithMany(p => p.Incomes)
                .HasForeignKey(i => i.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
