using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the 16-bit <c>INC</c> of a word held as an int: it wraps from 65535 to zero, steps every other value up by one,
/// and rejects a value outside a word.
/// </summary>
internal static class NativeWordIncrementVerification
{
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
}
