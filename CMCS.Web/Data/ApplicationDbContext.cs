using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMCS.Web.Models;

namespace CMCS.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Claim configuration
            builder.Entity<Claim>(entity =>
            {
                entity.HasOne(c => c.Lecturer)
                    .WithMany(u => u.Claims)
                    .HasForeignKey(c => c.LecturerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.SubmissionDate);
            });

            // Document configuration
            builder.Entity<Document>(entity =>
            {
                entity.HasOne(d => d.Claim)
                    .WithMany(c => c.Documents)
                    .HasForeignKey(d => d.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}