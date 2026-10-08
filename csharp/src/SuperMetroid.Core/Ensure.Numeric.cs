using System.Runtime.CompilerServices;

namespace SuperMetroid.Core;

/// <summary>Numeric argument checks. Inclusive bounds include both endpoints.</summary>
public static partial class Ensure
{
    /// <summary>Requires a value of at least zero.</summary>
    public static int AtLeastZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(parameterName, value, "Value must be at least zero.");

    /// <summary>Requires a value between the two bounds, inclusive.</summary>
    public static int BetweenInclusive(int minimum, int maximum, int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (minimum > maximum)
            throw new ArgumentException("Minimum must not exceed maximum.", nameof(minimum));
        if (value < minimum || value > maximum)
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {minimum} and {maximum}, inclusive.");
        return value;
    }
}
