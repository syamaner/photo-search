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
        modelBuilder
            .Entity<Photo>()
            .OwnsOne(product => product.Thumbnails, builder => { builder.ToJson(); })
            .OwnsOne(product => product.Metadata, builder => { builder.ToJson(); })
            .OwnsMany(product => product.PhotoSummaries, builder => { builder.ToJson(); });

        modelBuilder.Entity<Photo>()
            .Property(p => p.Height)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Width)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Latitude)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.Longitude)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.CaptureDate)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.ImportedDate)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.PublicUrl)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.RelativePath)
            .IsRequired();
        modelBuilder.Entity<Photo>()
            .Property(p => p.SizeKb)
            .IsRequired();
        
        modelBuilder.Entity<Photo>().HasKey(photo => photo.RelativePath);
    }
}