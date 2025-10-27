using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MBC.Persistence.Migrations;

public static class MigrationBuilderExtensions
{
    public static void RunFile(this MigrationBuilder migrationBuilder, string fileName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File {fileName} not found");
        }
        var content = File.ReadAllText(path);

        var batches = Regex.Split(content, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var batch in batches)
        {
            var trimmedBatch = batch.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedBatch))
            {
                migrationBuilder.Sql(trimmedBatch);
            }
        }
    }

    public static void RunFileOnSqlServer(this MigrationBuilder migrationBuilder, string fileName)
    {
        if (migrationBuilder.ActiveProvider?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            migrationBuilder.RunFile(fileName);
        }
    }
}
