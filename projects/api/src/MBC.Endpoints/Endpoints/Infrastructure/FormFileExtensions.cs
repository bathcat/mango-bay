
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MBC.Endpoints.Endpoints.Infrastructure;

public static class FormFileExtensions
{

    public static async Task<ReadOnlyMemory<byte>> ToMemory(this IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using var stream = file.OpenReadStream();
        var buffer = new byte[file.Length];
        await stream.ReadExactlyAsync(buffer);
        return buffer;
    }
}

