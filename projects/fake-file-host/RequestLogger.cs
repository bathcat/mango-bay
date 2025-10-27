using System.Text;

namespace FakeFileHost;

public static class RequestLogger
{
    public static void LogExfiltrationAttempt(HttpContext context, ILogger logger)
    {
        var request = context.Request;
        var connection = context.Connection;
        
        var logData = new StringBuilder();
        logData.AppendLine("=== DATA EXFILTRATION ATTEMPT DETECTED ===");
        logData.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        logData.AppendLine($"IP Address: {connection.RemoteIpAddress}");
        logData.AppendLine($"Full URL: {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
        logData.AppendLine($"Method: {request.Method}");
        logData.AppendLine($"User-Agent: {request.Headers.UserAgent}");
        logData.AppendLine($"Referer: {request.Headers.Referer}");
        logData.AppendLine($"Accept: {request.Headers.Accept}");
        logData.AppendLine($"Accept-Language: {request.Headers.AcceptLanguage}");
        logData.AppendLine($"Accept-Encoding: {request.Headers.AcceptEncoding}");
        
        if (request.Headers.Authorization.Count > 0)
        {
            logData.AppendLine($"Authorization: {request.Headers.Authorization}");
        }
        
        if (request.Headers.Cookie.Count > 0)
        {
            logData.AppendLine($"Cookies: {request.Headers.Cookie}");
        }
        
        if (request.Headers.ContainsKey("X-Forwarded-For"))
        {
            logData.AppendLine($"X-Forwarded-For: {request.Headers["X-Forwarded-For"]}");
        }
        
        if (request.Headers.ContainsKey("X-Real-IP"))
        {
            logData.AppendLine($"X-Real-IP: {request.Headers["X-Real-IP"]}");
        }
        
        if (request.Headers.ContainsKey("X-Custom-Header"))
        {
            logData.AppendLine($"X-Custom-Header: {request.Headers["X-Custom-Header"]}");
        }
        
        logData.AppendLine("=== ALL HEADERS ===");
        foreach (var header in request.Headers)
        {
            logData.AppendLine($"{header.Key}: {header.Value}");
        }
        
        logData.AppendLine("=== END EXFILTRATION LOG ===");
        logData.AppendLine();
        
        logger.LogInformation(logData.ToString());
    }
}
