using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MBC.Persistence.Tests;

public static class TestDbContextFactory
{
    public static MBCDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MBCDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new MBCDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    public static MBCDbContext CreateWithData(Action<MBCDbContext> seedAction)
    {
        MBCDbContext context = Create();
        seedAction(context);
        context.SaveChanges();
        return context;
    }

    public static ILogger<T> CreateLogger<T>()
    {
        return NullLogger<T>.Instance;
    }
}

