using System.Text.RegularExpressions;
using Vogen;

namespace MBC.Endpoints.Images;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct MimeTypeValue
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("MIME type cannot be empty.");
        }

        if (!Regex.IsMatch(value, @"^[a-z]+/[a-z0-9\-\+\.]+$", RegexOptions.IgnoreCase))
        {
            return Validation.Invalid(
                "MIME type must be in format 'type/subtype' (e.g., 'image/jpeg').");
        }

        return Validation.Ok;
    }
}

