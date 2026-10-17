using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hardware;
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
        AssertEqual(255, NativeWordCounter.DecrementByte(0), "a byte DEC wraps from zero to 255");
        AssertEqual(0, NativeWordCounter.DecrementByte(1), "a byte DEC reaches zero from one");
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            AssertEqual((int)NativeWordCounter.DecrementByte((byte)value), NativeWordCounter.DecrementByte(value),
                $"the int byte DEC matches the byte DEC at {value}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => NativeWordCounter.DecrementByte(256),
            "an int byte DEC rejects a value above a byte");
        AssertThrows<ArgumentOutOfRangeException>(() => NativeWordCounter.DecrementByte(-1),
            "an int byte DEC rejects a negative value");
    }

    /// <summary>Runs the focused native byte counter verification command.</summary>
    [VerificationCommand("--native-byte-counter")]
    internal static int NativeByteCounterCommand()
    {
        Suite(nameof(VerifyNativeByteCounter), () => VerifyNativeByteCounter());
        return 0;
    }
}
