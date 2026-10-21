using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the 8-bit <c>DEC</c> of a byte held as an int: it wraps from zero to 255 exactly as the byte form does, steps
/// every other value down by one, and rejects a value outside a byte.
/// </summary>
internal static class NativeByteCounterVerification
{
    /// <summary>Checks the int byte decrement against the byte form across the whole byte range.</summary>
    internal static void VerifyNativeByteCounter()
    {
        AssertEqual(255, WrappingCounter.DecrementByte(0), "a byte DEC wraps from zero to 255");
        AssertEqual(0, WrappingCounter.DecrementByte(1), "a byte DEC reaches zero from one");
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            AssertEqual(WrappingCounter.DecrementByte((byte)value), WrappingCounter.DecrementByte(value),
                $"the int byte DEC matches the byte DEC at {value}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.DecrementByte(256),
            "an int byte DEC rejects a value above a byte");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.DecrementByte(-1),
            "an int byte DEC rejects a negative value");
    }

    /// <summary>Checks the 16-bit <c>INC</c> of a word held as an int against the word form across the whole word range.</summary>
    internal static void VerifyNativeWordIncrement()
    {
        AssertEqual(0, WrappingCounter.IncrementWord(ushort.MaxValue), "a word INC wraps from 65535 to zero");
        AssertEqual(1, WrappingCounter.IncrementWord(0), "a word INC steps zero to one");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            AssertEqual(unchecked((ushort)(value + 1)), WrappingCounter.IncrementWord(value),
                $"the int word INC matches the 16-bit INC at {value}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.IncrementWord(ushort.MaxValue + 1),
            "an int word INC rejects a value above sixteen bits");
        AssertThrows<ArgumentOutOfRangeException>(() => WrappingCounter.IncrementWord(-1),
            "an int word INC rejects a negative value");
    }

    /// <summary>Runs the focused native word increment verification command.</summary>
    [VerificationCommand("--native-word-increment")]
    internal static int NativeWordIncrementCommand()
    {
        Suite(nameof(VerifyNativeWordIncrement), () => VerifyNativeWordIncrement());
        return 0;
    }

    /// <summary>Runs the focused native byte counter verification command.</summary>
    [VerificationCommand("--native-byte-counter")]
    internal static int NativeByteCounterCommand()
    {
        Suite(nameof(VerifyNativeByteCounter), () => VerifyNativeByteCounter());
        return 0;
    }
}
