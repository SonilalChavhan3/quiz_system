using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using quiz_system.Models.QuizModel;
using System.Collections.Generic;

namespace quiz_system.Models
{
    public class QuizContext : IdentityDbContext<ApplicationUser>
    {
        public QuizContext(DbContextOptions<QuizContext> options) : base(options) { }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserQuizResult> UserQuizResults { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed sample Sections and Questions (you can modify or remove this as needed)
            modelBuilder.Entity<Section>().HasData(
                new Section { Id = 1, Name = "General Knowledge", TimeLimitInMinutes = 5 }
            );

            modelBuilder.Entity<Question>().HasData(
                new Question { Id = 1, SectionId = 1, QuestionText = "What is the capital of France?", OptionA = "Berlin", OptionB = "Paris", OptionC = "Madrid", OptionD = "Rome", CorrectAnswer = "OptionB" },
                new Question { Id = 2, SectionId = 1, QuestionText = "Which planet is known as the Red Planet?", OptionA = "Earth", OptionB = "Venus", OptionC = "Mars", OptionD = "Jupiter", CorrectAnswer = "OptionC" }
            );

            SeedUsers(modelBuilder);
        }
        private void SeedUsers(ModelBuilder builder)
        {
            var hasher = new PasswordHasher<ApplicationUser>();

            // Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "3c2e38ac-373f-4519-b648-43c21c518b21", Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
                new IdentityRole { Id = "8ff3118b-0086-4e2d-8270-cf2a86f0a55f", Name = "Admin", NormalizedName = "ADMIN" },
                 new IdentityRole { Id = "d3cf1b10-b5a8-4cab-82c7-a3b0becb6221", Name = "User", NormalizedName = "USER" }
            );

            // Seed Admin User
            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "155a39b8-f44d-4e9d-8a60-3af8c3453e1d",
                    UserName = "admin@admin.com",
                    NormalizedUserName = "ADMIN@ADMIN.COM",
                    Email = "admin@admin.com",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "Admin@1234"),
                    SecurityStamp = Guid.NewGuid().ToString("D")
                }
            );

            // Seed Regular User
            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "2e8cb337-0be4-4f90-b4ab-8d447ad168a3",
                    UserName = "user@user.com",
                    NormalizedUserName = "USER@USER.COM",
                    Email = "user@user.com",
                    NormalizedEmail = "USER@USER.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, "User@1234"),
                    SecurityStamp = Guid.NewGuid().ToString("D")
                }
            );
        }

    }

}
