using SuperMetroid.Core.Bosses.Torizo;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Confirms that the Torizos' named attack-choice phase (#1619) chooses the first attack on exactly the updates the mask
/// expression it replaces did: bit three of the frame counter plus Samus's X halved, with the bit the halving dropped
/// added back.
/// </summary>
internal static class TorizoAttackPhaseVerification
{
    /// <summary>Samus positions compared at every frame counter: the left edge, odd and even, the movie's landing X and negative positions.</summary>
    private static readonly int[] SampledSamusXs = [0, 1, 2, 3, 15, 16, 255, 440, 511, 4095, -1, -2, -3, -16, -17];

    /// <summary>Compares the phase with the replaced expression over every frame counter and over a full range of Samus positions.</summary>
    internal static void VerifyTorizoAttackPhase()
    {
        foreach (int samusX in SampledSamusXs)
        {
            string rule = $"first attack phase at X {samusX}";
            for (int frameCounter = 0; frameCounter <= ushort.MaxValue; frameCounter++)
                AssertSameAt(Replaced(frameCounter, samusX), TorizoChoices.InFirstAttackPhase(frameCounter, samusX), rule, frameCounter);
        }

        for (int frameCounter = 0; frameCounter < 16; frameCounter++)
        {
            string rule = $"first attack phase at frame {frameCounter}";
            for (int samusX = -4096; samusX <= ushort.MaxValue; samusX++)
                AssertSameAt(Replaced(frameCounter, samusX), TorizoChoices.InFirstAttackPhase(frameCounter, samusX), rule, samusX);
        }

        Console.WriteLine("Torizo attack phase: every frame counter and Samus position chooses as the mask expression it replaces.");
    }

    /// <summary>The replaced expression: the frame counter plus Samus's X halved and its dropped bit, tested at bit three.</summary>
    private static bool Replaced(int frameCounter, int samusX) => ((frameCounter + (samusX & 1) + (samusX >> 1)) & 8) != 0;
}
