using System.IO;
using System.IO.Abstractions;
using MBC.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MBC.Endpoints.Images;

public static class ImageExtensions
{
    public static IHostApplicationBuilder AddImageStorage(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ImageStorageOptions>(
            builder.Configuration.GetSection(ImageStorageOptions.SectionName));

        var fs = new FileSystem();
        builder.Services.AddSingleton<IFile>(fs.File);
        builder.Services.AddSingleton<IDirectory>(fs.Directory);
        builder.Services.AddSingleton<IFileStreamFactory>(fs.FileStream);
        builder.Services.AddSingleton<IPath>(fs.Path);

        builder.Services.AddScoped<IImageRepository, FileSystemImageRepository>();

        return builder;
    }

    public static WebApplication UseImageStaticFiles(this WebApplication app)
    {
        const string uploadDirectory = "/uploads";
        const string assetsDirectory = "assets";

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, assetsDirectory, "uploads")),
            RequestPath = uploadDirectory,
            ServeUnknownFileTypes = false,
        });

        return app;
    }
}
