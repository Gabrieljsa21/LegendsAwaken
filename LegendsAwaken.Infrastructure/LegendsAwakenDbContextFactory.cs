using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class LegendsAwakenDbContextFactory : IDesignTimeDbContextFactory<LegendsAwakenDbContext>
{
    public LegendsAwakenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LegendsAwakenDbContext>();
        optionsBuilder.UseSqlite("Data Source=legendsawaken.db");

        return new LegendsAwakenDbContext(optionsBuilder.Options);
    }
}
