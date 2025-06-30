using Microsoft.EntityFrameworkCore;
using OnlineExamPortalFinal.Models;


namespace OnlineExamPortalFinal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //cascade paths
            modelBuilder.Entity<Response>()
                .HasOne(r => r.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Exam)
                .WithMany()
                .HasForeignKey(r => r.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Question)
                .WithMany()
                .HasForeignKey(r => r.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Report relationships
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Exam)
                .WithMany()
                .HasForeignKey(r => r.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Name = "Admin",
                Email = "admin@examportal.com",
                PasswordHash = "$2a$11$h6zWmyEYcHZLWynqVnjcqO3bJMOXM7N5MuxqLGgIl1qzMzP4zhRUC", 
                Role = "Admin"
            });
        }
    }
}
