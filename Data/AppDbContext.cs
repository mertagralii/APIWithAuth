using APIWithAuth.Entities;
using APIWithAuth.Model.Dto.Comment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APIWithAuth.Data;

public class AppDbContext: IdentityDbContext<ApplicationUser> // Buraya dikkat !
{
    public DbSet<Tweet> Tweets { get; set; }
    public DbSet<Comment> Comments { get; set; } 
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tweet silinince Comment'lar silinsin
        builder.Entity<Comment>()
            .HasOne(c => c.Tweet)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TweetId)
            .OnDelete(DeleteBehavior.Cascade);

        // User silinse bile Comment'lar silinmesin (çakışmayı engellemek için bu önemli!)
        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User silinince Tweet’ler silinsin
        builder.Entity<Tweet>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tweets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    
    
    
}