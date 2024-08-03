using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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