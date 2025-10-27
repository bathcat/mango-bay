using System.Collections.Generic;
using System.Linq;
using Vogen;

namespace MBC.Core.ValueObjects;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct Fingerprint
{
    public static Fingerprint From(IEnumerable<(string Label, string Value)> items)
    {
        var parts = items.Select(i => $"{i.Label}={i.Value}");
        var combined = string.Join(";", parts);
        return From(combined);
    }

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Fingerprint cannot be empty.");
        }

        if (value.Length > 1000)
        {
            return Validation.Invalid("Fingerprint exceeds maximum length of 1000 characters.");
        }

        return Validation.Ok;
    }
}

