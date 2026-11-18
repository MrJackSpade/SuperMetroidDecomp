using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// Confirms that the chance Samus's breath bubbles make the low sound (#2479) decides every one of the 65536 samples
/// exactly as the test of the sample's lowest bit it replaces did.
/// </summary>
internal static class SamusBreathBubbleSoundVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifySamusBreathBubbleSound()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, SamusBreathBubbleDefinitions.SoundsLow.IsTakenBy(sample), "low bubbling sound", sample);

        Console.WriteLine("Samus breath bubble sound: all 65536 samples decide as the bit test it replaces.");
    }
}
