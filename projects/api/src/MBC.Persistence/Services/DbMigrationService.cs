using System.Threading;
using System.Threading.Tasks;
using MBC.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MBC.Persistence.Services;

public class DbMigrationService : IDbMigrationService
{
    private readonly MBCDbContext _context;
    private readonly IConfiguration _configuration;

    public DbMigrationService(MBCDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task Migrate(CancellationToken cancellationToken = default)
    {
        if (_context.Database.IsSqlServer())
        {
            var setupConnectionString = _configuration.GetValue<string>("Database:SetupConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<MBCDbContext>();
            optionsBuilder.UseSqlServer(setupConnectionString);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            using var setupContext = new MBCDbContext(optionsBuilder.Options);
            await setupContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _context.Database.MigrateAsync(cancellationToken);
        }
    }
}
