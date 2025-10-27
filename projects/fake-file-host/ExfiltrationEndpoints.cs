using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace FakeFileHost;

public static class ExfiltrationEndpoints
{
    public static void MapExfiltrationEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/fonts/{**path}", ServeFont)
            .WithName("ServeFont")
            .WithDescription("Serves Montserrat font files for data exfiltration demonstration");

        app.MapGet("/images/{**path}", ServeImage)
            .WithName("ServeImage")
            .WithDescription("Serves background image for data exfiltration demonstration");

        app.MapGet("/styles/{**path}", ServeStyles)
            .WithName("ServeStyles")
            .WithDescription("Serves CSS file for data exfiltration demonstration");

        app.MapGet("/scripts/{**path}", ServeScripts)
            .WithName("ServeScripts")
            .WithDescription("Serves JavaScript file for data exfiltration demonstration");
    }

    public static IResult ServeFont(HttpContext context, ILogger<Program> logger)
    {
        RequestLogger.LogExfiltrationAttempt(context, logger);
        
        var fontPath = Path.Combine("assets", "Montserrat", "Montserrat-VariableFont_wght.ttf");
        var fileStream = File.OpenRead(fontPath);
        return Results.File(fileStream, "font/ttf", "Montserrat-VariableFont_wght.ttf");
    }

    public static IResult ServeImage(HttpContext context, ILogger<Program> logger)
    {
        RequestLogger.LogExfiltrationAttempt(context, logger);
        
        var imagePath = Path.Combine("assets", "backrground.png");
        var fileStream = File.OpenRead(imagePath);
        return Results.File(fileStream, "image/png", "background.png");
    }

    public static IResult ServeStyles(HttpContext context, ILogger<Program> logger)
    {
        RequestLogger.LogExfiltrationAttempt(context, logger);
        
        var stylesPath = Path.Combine("assets", "styles.css");
        var fileStream = File.OpenRead(stylesPath);
        return Results.File(fileStream, "text/css", "styles.css");
    }

    public static IResult ServeScripts(HttpContext context, ILogger<Program> logger)
    {
        RequestLogger.LogExfiltrationAttempt(context, logger);
        
        var scriptPath = Path.Combine("assets", "script.js");
        var fileStream = File.OpenRead(scriptPath);
        return Results.File(fileStream, "application/javascript", "script.js");
    }
}
