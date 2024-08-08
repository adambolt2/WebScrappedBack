using Microsoft.EntityFrameworkCore;
using WebScrappedBack.Models.Entities;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IndeedModel> IndeedModels { get; set; }
        public DbSet<LinkedInModel> LinkedInModels { get; set; }
        public DbSet<TotalJobsModel> TotalJobsModels { get; set; }
        public DbSet<Users> Users { get; set; } // Added Users DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Additional configuration (if any) can go here

            // Example: Setting up unique constraint for ApiKey
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.ApiKey)
                .IsUnique();

            // Other configurations...
        }
    }
}
