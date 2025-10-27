using MBC.Core;
using MBC.Core.ValueObjects;

namespace MBC.Services.Seeds;

public class SeedFingerprintService : IFingerprintService
{
    public Fingerprint GenerateFingerprint()
    {
        return Fingerprint.From([("source", "seed"), ("context", "database-initialization")]);
    }
}

