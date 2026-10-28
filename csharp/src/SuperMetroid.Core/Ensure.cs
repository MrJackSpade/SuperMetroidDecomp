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

    /// <summary>
    /// Requires a key the table lists and returns its entry: the lookup a catalog's identity table answers, rejecting an
    /// identity the table does not hold.
    /// </summary>
    public static TValue Listed<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> table, TKey key,
        [CallerArgumentExpression(nameof(key))] string? parameterName = null)
        where TKey : notnull =>
        NotNull(table).TryGetValue(key, out TValue value)
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, key, "Value is not listed.");

    /// <summary>Requires an index within <paramref name="items"/> and returns its element.</summary>
    public static T Element<T>(IReadOnlyList<T> items, int index,
        [CallerArgumentExpression(nameof(index))] string? parameterName = null) =>
        (uint)index < (uint)NotNull(items).Count
            ? items[index]
            : throw new ArgumentOutOfRangeException(parameterName, index, $"Index must be below {items.Count}.");

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
