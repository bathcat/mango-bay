using System;
using System.Collections.Generic;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;
using Sanitizer = Ganss.Xss.HtmlSanitizer;

namespace MBC.Services;

public class HtmlSanitizer : IHtmlSanitizer
{
    private static readonly IEnumerable<string> AllowedTags =
    [
        "p", "br", "strong", "em", "u", "ul", "ol", "li"
    ];

    private readonly Sanitizer _sanitizer;
    private readonly ILogger<HtmlSanitizer> _logger;

    public HtmlSanitizer(ILogger<HtmlSanitizer> logger)
    {
        _logger = logger;
        _sanitizer = GetSanitizer();
    }

    public string Sanitize(string original)
    {
        ArgumentNullException.ThrowIfNull(original, nameof(original));

        var sanitized = _sanitizer.Sanitize(original);

        if (original != sanitized)
        {
            _logger.LogWarning(
                "HTML content was sanitized. Original length: {OriginalLength}, Sanitized length: {SanitizedLength}",
                original.Length,
                sanitized.Length);
        }

        return sanitized;
    }

    private static Sanitizer GetSanitizer()
    {
        var sanitizer = new Sanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();

        foreach (var tag in AllowedTags)
        {
            sanitizer.AllowedTags.Add(tag);
        }

        return sanitizer;
    }

}
