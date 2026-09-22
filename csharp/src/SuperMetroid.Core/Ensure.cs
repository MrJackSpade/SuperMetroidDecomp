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

    /// <summary>Requires a nullable value type to contain a value, returning its underlying type.</summary>
    public static T NotNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : struct =>
        value ?? throw new ArgumentNullException(parameterName);

    /// <summary>Requires a non-null, nonempty string.</summary>
    public static string NotNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (value.Length == 0)
            throw new ArgumentException("Value must not be empty.", parameterName);
        return value;
    }

    /// <summary>Requires a string containing at least one non-whitespace character.</summary>
    public static string NotNullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value must not be empty or whitespace.", parameterName);
        return value;
    }

    /// <summary>Requires the value to equal the expected value.</summary>
    public static T Equal<T>(
        T value,
        T expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(value, expected))
            throw new ArgumentException($"Value must equal {expected}.", parameterName);
        return value;
    }

    /// <summary>Requires the value to differ from a disallowed value.</summary>
    public static T NotEqual<T>(
        T value,
        T disallowed,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (EqualityComparer<T>.Default.Equals(value, disallowed))
            throw new ArgumentException($"Value must differ from {disallowed}.", parameterName);
        return value;
    }

    /// <summary>Requires one of two allowed values.</summary>
    public static T OneOf<T>(
        T value,
        T first,
        T second,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(value, first) &&
            !EqualityComparer<T>.Default.Equals(value, second))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value is not in the allowed set.");
        return value;
    }

    /// <summary>Requires membership in the supplied nonempty set.</summary>
    public static T OneOf<T>(
        T value,
        IReadOnlyCollection<T> allowed,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(allowed);
        if (allowed.Count == 0)
            throw new ArgumentException("Allowed set must not be empty.", nameof(allowed));
        if (!allowed.Contains(value))
            throw new ArgumentOutOfRangeException(parameterName, value, "Value is not in the allowed set.");
        return value;
    }

    /// <summary>Requires a closed enum member. Unnamed flag combinations need a separate domain rule.</summary>
    public static TEnum IsDefined<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentOutOfRangeException(
                parameterName, value, $"Value '{value}' is not defined for {typeof(TEnum).Name}.");
        return value;
    }

    /// <summary>Requires an array of exactly the specified length.</summary>
    public static T[] LengthEqual<T>(
        T[]? value,
        int expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (value.Length != expected)
            throw new ArgumentException($"Length must equal {expected}.", parameterName);
        return value;
    }

    /// <summary>Requires a string of exactly the specified length.</summary>
    public static string LengthEqual(
        string? value,
        int expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (value.Length != expected)
            throw new ArgumentException($"Length must equal {expected}.", parameterName);
        return value;
    }

    /// <summary>Requires a span of exactly the specified length.</summary>
    public static Span<T> LengthEqual<T>(
        Span<T> value,
        int expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value.Length != expected)
            throw new ArgumentException($"Length must equal {expected}.", parameterName);
        return value;
    }

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

    /// <summary>Requires a collection of exactly the specified count.</summary>
    public static IReadOnlyCollection<T> CountEqual<T>(
        IReadOnlyCollection<T>? value,
        int expected,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (value.Count != expected)
            throw new ArgumentException($"Count must equal {expected}.", parameterName);
        return value;
    }
}
