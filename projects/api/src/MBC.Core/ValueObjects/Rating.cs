using Vogen;

namespace MBC.Core.ValueObjects;

[ValueObject<int>(conversions: Conversions.SystemTextJson)]
public partial struct Rating
{
    private static Validation Validate(int value)
    {
        if (value < 1 || value > 5)
        {
            return Validation.Invalid("Rating must be between 1 and 5 stars.");
        }

        return Validation.Ok;
    }
}

