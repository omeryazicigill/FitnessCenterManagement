using FitnessCenterManagement.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser Configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Weight)
                    .HasColumnType("decimal(18,2)");
            });

            // Gym Configuration
            builder.Entity<Gym>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Name);
            });

            // Service Configuration
            builder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Gym)
                    .WithMany(g => g.Services)
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Trainer Configuration
            builder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Gym)
                    .WithMany(g => g.Trainers)
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TrainerService Configuration (Many-to-Many)
            builder.Entity<TrainerService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.TrainerServices)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.TrainerServices)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // TrainerAvailability Configuration
            builder.Entity<TrainerAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Availabilities)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment Configuration
            builder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Appointments)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasIndex(e => e.AppointmentDate);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}




