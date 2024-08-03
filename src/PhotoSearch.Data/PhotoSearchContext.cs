using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Data;

public class PhotoSearchContext : DbContext
{
    public PhotoSearchContext(DbContextOptions<PhotoSearchContext> opt, IConfiguration configuration) : base(opt)
    {
    }

    public DbSet<Photo> Photos => Set<Photo>();
 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Photo>()
            .OwnsOne(c => c.Metadata, d =>
            {
                d.ToJson();
            });
        modelBuilder.Entity<Photo>()
            .OwnsOne(c => c.PhotoSummaries, d =>
            {
                d.ToJson();
            });
        
        modelBuilder.Entity<Photo>()
            .Property(p => p.LocationInformation)
            .HasColumnType("jsonb")
            .IsRequired(false);

        modelBuilder.Entity<Photo>()
            .OwnsOne(c => c.Thumbnails, d =>
            {
                d.ToJson();
            });
        
        modelBuilder.Entity<Photo>()
            .Property(p => p.Height)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Width)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Latitude)
            .IsRequired(false).HasDefaultValue(null);;
        modelBuilder.Entity<Photo>()
            .Property(p => p.Longitude)
            .IsRequired(false).HasDefaultValue(null);
        modelBuilder.Entity<Photo>()
            .Property(p => p.CaptureDateUtc)
            .IsRequired(false).HasDefaultValue(null);
        modelBuilder.Entity<Photo>()
            .Property(p => p.ImportedDateUtc)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.PublicUrl)
            .IsRequired(false).HasDefaultValue(null);;
        modelBuilder.Entity<Photo>()
            .Property(p => p.RelativePath)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.ExactPath)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.FileType)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.SizeKb)
            .IsRequired();
        
        modelBuilder.Entity<Photo>().HasKey(photo => photo.RelativePath);
    }
}