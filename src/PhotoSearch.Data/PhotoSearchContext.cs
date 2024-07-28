using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Data;
public class PhotoSearchContextFactory : IDesignTimeDbContextFactory<PhotoSearchContext>
{
    public PhotoSearchContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PhotoSearchContext>();
        optionsBuilder.UseNpgsql();
        return new PhotoSearchContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}
public class PhotoSearchContext : DbContext
{
    public PhotoSearchContext(DbContextOptions<PhotoSearchContext> opt, IConfiguration configuration) : base(opt)
    {
    }

    public DbSet<Photo> Photos => Set<Photo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
 
        modelBuilder.Entity<Photo>()
            .Property(p => p.Metadata)
            .HasColumnType("jsonb")
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Thumbnails)
            .HasColumnType("jsonb")
            .IsRequired(false);
        modelBuilder.Entity<Photo>()
            .Property(p => p.PhotoSummaries)
            .HasColumnType("jsonb")
            .IsRequired(false);
        modelBuilder.Entity<Photo>()
            .Property(p => p.LocationInformation)
            .HasColumnType("jsonb")
            .IsRequired(false);

        
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
            .Property(p => p.CaptureDateUTC)
            .IsRequired(false).HasDefaultValue(null);
        modelBuilder.Entity<Photo>()
            .Property(p => p.ImportedDateUTC)
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