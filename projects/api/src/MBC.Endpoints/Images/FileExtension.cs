using System.Text.RegularExpressions;
using Vogen;

namespace MBC.Endpoints.Images;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct FileExtension
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("File extension cannot be empty.");
        }

        if (!value.StartsWith('.'))
        {
            return Validation.Invalid("File extension must start with a period (e.g., '.jpg').");
        }

        if (value.Length < 2 || value.Length > 11)
        {
            return Validation.Invalid("File extension must be between 2 and 11 characters (e.g., '.jpg').");
        }

        if (!Regex.IsMatch(value, @"^\.[a-zA-Z0-9]+$"))
        {
            return Validation.Invalid(
                "File extension contains invalid characters. Only alphanumeric characters are allowed after the period.");
        }

        return Validation.Ok;
    }
}

