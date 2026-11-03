using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.TestSupport.Assertions;

/// <summary>
/// Dependency-free assertions shared by every verifier and the shared verification source it compiles.
/// </summary>
/// <remarks>
/// Native-state properties are intentionally narrow (<see cref="byte"/>,
/// <see cref="ushort"/>, and <see cref="short"/>), while C# numeric literals and loop
/// expressions naturally begin as <see cref="int"/>. The numeric overloads centralize
/// that checked narrowing so tests can say <c>AssertEqual(0x8000, state.Word, ...)</c>
/// without repeating casts at hundreds of call sites. An out-of-range expectation fails
/// as bad fixture data instead of silently wrapping before the comparison.
/// </remarks>
internal static class VerificationAssertions
{
    /// <summary>Fails a verification with its supplied context when a required condition is false.</summary>
    internal static void AssertTrue(bool condition, string context)
    {
        if (!condition)
            throw new InvalidOperationException($"Verification failed: {context}.");
    }

    /// <summary>Asserts that <paramref name="value"/> is a <typeparamref name="T"/> and returns it as one.</summary>
    internal static T AssertIs<T>(object? value, string context) =>
        value is T typed ? typed : throw new InvalidOperationException($"Verification failed: {context} (found {value}).");

    /// <summary>Compares arbitrary values with the default equality comparer and reports both values on failure.</summary>
    internal static void AssertEqual<T>(T expected, T actual, string context)
    {
        // EqualityComparer<T>.Default handles primitives, records, nullable values, and
        // enums. Constraining T to IEquatable<T> looks attractive but excludes C# enums.
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"Verification failed: {context}; expected {expected}, got {actual}.");
        }
    }

    /// <summary>Checks that an integer expectation fits a byte before comparing it.</summary>
    internal static void AssertEqual(int expected, byte actual, string context) =>
        AssertEqual<byte>(checked((byte)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a signed byte before comparing it.</summary>
    internal static void AssertEqual(int expected, sbyte actual, string context) =>
        AssertEqual<sbyte>(checked((sbyte)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a native word before comparing it.</summary>
    internal static void AssertEqual(int expected, ushort actual, string context) =>
        AssertEqual<ushort>(checked((ushort)expected), actual, context);

    /// <summary>Checks that an integer expectation fits a signed word before comparing it.</summary>
    internal static void AssertEqual(int expected, short actual, string context) =>
        AssertEqual<short>(checked((short)expected), actual, context);

    /// <summary>Checks a nullable native word against a safely narrowed integer expectation.</summary>
    internal static void AssertEqual(int expected, ushort? actual, string context) =>
        AssertEqual<ushort?>(checked((ushort)expected), actual, context);

    /// <summary>Compares a color with an expected cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(int expectedWord, RgbColor actual, string context) =>
        AssertEqual(RgbColor.FromWord(checked((ushort)expectedWord)), actual, context);

    /// <summary>Compares an expected color with an actual cartridge word, decoded as a 15-bit color.</summary>
    internal static void AssertEqual(RgbColor expected, ushort actualWord, string context) =>
        AssertEqual(expected, RgbColor.FromWord(actualWord), context);

    /// <summary>Compares a nullable color with an expected cartridge word.</summary>
    internal static void AssertEqual(int expectedWord, RgbColor? actual, string context) =>
        AssertEqual<RgbColor?>(RgbColor.FromWord(checked((ushort)expectedWord)), actual, context);

    /// <summary>Returns the expected exception, or fails when the action completes without it.</summary>
    internal static TException AssertThrows<TException>(Action action, string context)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException(
            $"Verification failed: {context}; expected {typeof(TException).Name}.");
    }

    /// <summary>
    /// Returns the expected exception from a construction or query, or fails when it yields a value instead. The
    /// evaluated value is the outcome under test, so a constructor or query that should reject its input is passed here.
    /// </summary>
    internal static TException AssertThrows<TException>(Func<object?> evaluate, string context)
        where TException : Exception
    {
        object? produced;
        try
        {
            produced = evaluate();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException(
            $"Verification failed: {context}; expected {typeof(TException).Name}, but it produced {produced ?? "null"}.");
    }
}
