using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceLibrary.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Factory untuk konfigurasi waktu desain
    // Constructor
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost; Database=postgres; Username=mastama; Password=post456;");

        return new AppDbContext(optionsBuilder.Options);
    }
}