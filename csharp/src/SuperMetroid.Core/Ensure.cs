using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core;

/// <summary>Reusable validation of caller-supplied values. Every operation returns the validated value.</summary>
public static partial class Ensure
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
}
