using Microsoft.EntityFrameworkCore;
using developers.Models;
using System.Collections.Generic;

namespace developers.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Developer> Developers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectDeveloper> ProjectDevelopers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TaskDeveloper> TaskDevelopers { get; set; }
        public DbSet<TaskCard> TaskCards { get; set; }
        public DbSet<Comments> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Developer>()
                .HasMany(p => p.Projects)
                .WithMany(t => t.Developers)
                .UsingEntity<ProjectDeveloper>(
                    j => j
                        .HasOne(pt => pt.Project)
                        .WithMany(t => t.ProjectDevelopers)
                        .HasForeignKey(pt => pt.ProjectID)
                        .OnDelete(DeleteBehavior.Cascade),

                    j => j
                        .HasOne(pt => pt.Developer)
                        .WithMany(p => p.ProjectDevelopers)
                        .HasForeignKey(pt => pt.DeveloperID)
                        .OnDelete(DeleteBehavior.Cascade),

                    j =>
                    {
                        j.HasKey(t => new { t.DeveloperID, t.ProjectID });
                    }
                );

            modelBuilder.Entity<Developer>()
                .HasMany(p => p.Tasks)
                .WithMany(t => t.Developers)
                .UsingEntity<TaskDeveloper>(
                    j => j
                        .HasOne(pt => pt.Task)
                        .WithMany(t => t.TaskDevelopers)
                        .HasForeignKey(pt => pt.TaskId)
                        .OnDelete(DeleteBehavior.Cascade),

                    j => j
                        .HasOne(pt => pt.Developer)
                        .WithMany(p => p.TaskDevelopers)
                        .HasForeignKey(pt => pt.DeveloperId)
                        .OnDelete(DeleteBehavior.Cascade),

                    j =>
                    {
                        j.HasKey(t => new { t.DeveloperId, t.TaskId });
                    }
                );

            modelBuilder.Entity<User>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(a => a.Developer)
                .WithOne(d => d.user)
                .HasForeignKey<Developer>(d => d.UserID);

            modelBuilder.Entity<TaskCard>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Users)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
