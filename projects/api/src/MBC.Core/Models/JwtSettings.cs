namespace MBC.Core.Models;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 15;

    public int RefreshTokenExpirationDays { get; set; } = 30;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}

