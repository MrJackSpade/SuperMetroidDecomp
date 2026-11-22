using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Metroids.MetroidNativeListAddresses;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// #641, #1600: the Metroid's typed animations, each segment matched to its native bank-$A3 list and each animation to
/// the lists' control flow.
/// </summary>
internal static class MetroidAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMetroidAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, Entries,
            MetroidAnimations.Segments, MetroidAnimations.StartingAt, MetroidFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MetroidCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }),
            // The chasing list writes its pulse out four times (#2337), which the typed animation plays as four plays of
            // one pulse before the cry; the cry only follows them.
            onlyRunOnInto: new HashSet<MetroidSegment> { MetroidSegment.ChasingCry });
        Console.WriteLine($"  Metroid animations: {Entries.Length} segments ({beats} beats) and {MetroidAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
