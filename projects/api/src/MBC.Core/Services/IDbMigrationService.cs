using System.Threading;
using System.Threading.Tasks;

namespace MBC.Core.Services;

public interface IDbMigrationService
{
    Task Migrate(CancellationToken cancellationToken = default);
}
