using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core;

/// <summary>Reusable validation of caller-supplied values. Every operation returns the validated value.</summary>
public static class Ensure
{
    /// <summary>Requires a non-null reference and reports the caller's value expression.</summary>
    public static T NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class =>
        value ?? throw new ArgumentNullException(parameterName);

    /// <summary>Requires a read-only span of exactly the specified length.</summary>
    public static ReadOnlySpan<T> LengthEqual<T>(
        ReadOnlySpan<T> value,
        int expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value.Length != expected)
            throw new ArgumentException($"Length must equal {expected}.", parameterName);
        return value;
    }

    /// <summary>Requires a value of at least zero.</summary>
    public static int AtLeastZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(parameterName, value, "Value must be at least zero.");

    /// <summary>Requires a value of at least zero.</summary>
    public static long AtLeastZero(long value,
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
