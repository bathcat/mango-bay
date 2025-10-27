using System;
using System.Linq;
using System.Net;
using MBC.Core;
using MBC.Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MBC.Endpoints.Security;

public class FingerprintService : IFingerprintService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FingerprintService> _logger;

    public FingerprintService(IHttpContextAccessor httpContextAccessor, ILogger<FingerprintService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Generates a fingerprint for the current HTTP context.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// We're balancing UX v security. This implementation may be
    /// somewhat brittle. e.g.
    /// * Switch networks (home → work → coffee shop)
    /// * VPN on/off
    /// * Browser updates
    /// * Use mobile devices on cellular data v home wifi
    ///
    /// That's fine. We'll sort it out in the UAT phase if
    /// we get too many complaints.
    /// </remarks>
    /// <exception cref="InvalidOperationException"></exception>
    public Fingerprint GenerateFingerprint()
    {
        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available");

        var ipSubnet = GetIpAddress(context).GetSubnet().ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();

        _logger.LogDebug("Generated fingerprint for IP subnet: {IpSubnet}", ipSubnet);

        return Fingerprint.From([
            ("ipSubnet", ipSubnet),
            ("userAgent", userAgent),
            ("acceptLanguage", acceptLanguage)
        ]);
    }

    private IPAddress GetIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ipString = forwardedFor.Split(',')[0].Trim();
            if (IPAddress.TryParse(ipString, out var parsed))
            {
                return parsed;
            }
        }

        return context.Connection.RemoteIpAddress
            ?? throw new InvalidOperationException("Unable to determine client IP address");
    }
}

