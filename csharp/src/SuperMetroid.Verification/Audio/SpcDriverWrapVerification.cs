using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the audio unit's counters stored as ints: each byte and word wraps at both ends exactly as the SPC700's
/// byte and word do, and a driver-state snapshot keeps its one-byte and low-byte-first layout and rejects a value
/// outside its driver width rather than producing a different snapshot.
/// </summary>
internal static class SpcDriverWrapVerification
{
    /// <summary>Checks the byte and word wraps and the snapshot layout.</summary>
    internal static void VerifySpcDriverWrap()
    {
        AssertEqual(255, SpcDriverWrap.Byte(0 - 1), "a byte countdown wraps from zero to 255");
        AssertEqual(0, SpcDriverWrap.Byte(255 + 1), "a byte count wraps from 255 to zero");
        AssertEqual(0x40, SpcDriverWrap.Byte(0xA0 << 1), "a byte shift drops the carried-out bit");
        AssertEqual(65535, SpcDriverWrap.Word(0 - 1), "a word countdown wraps from zero to 65535");
        AssertEqual(1, SpcDriverWrap.Word(65535 + 2), "a word pointer advance wraps past $FFFF");

        var state = new List<byte>();
        SpcDriverSnapshot.AddBytes(state, [0, 255, 0x81]);
        SpcDriverSnapshot.AddWords(state, [0x1234, 65535]);
        AssertSequenceEqual(new byte[] { 0, 255, 0x81, 0x34, 0x12, 0xFF, 0xFF }, state,
            "snapshot bytes are single bytes and words are low byte first");
        AssertThrows<OverflowException>(() => SpcDriverSnapshot.AddBytes(state, [256]),
            "a snapshot byte outside a driver byte is rejected");
        AssertThrows<OverflowException>(() => SpcDriverSnapshot.AddWords(state, [-1]),
            "a snapshot word outside a driver word is rejected");
    }

    /// <summary>Runs the focused SPC driver wrap verification command.</summary>
    [VerificationCommand("--spc-driver-wrap")]
    internal static int SpcDriverWrapCommand()
    {
        Suite(nameof(VerifySpcDriverWrap), () => VerifySpcDriverWrap());
        return 0;
    }
}
