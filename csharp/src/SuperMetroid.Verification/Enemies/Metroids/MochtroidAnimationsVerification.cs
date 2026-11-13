using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Metroids.MochtroidNativeListAddresses;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// #641, #1600: Mochtroid's typed animations, each segment matched to its native bank-$A3 list and each animation to
/// the lists' control flow.
/// </summary>
internal static class MochtroidAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMochtroidAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MochtroidAnimations.Segments, MochtroidAnimations.StartingAt, MochtroidFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Mochtroid animations: {Entries.Length} segments ({beats} beats) and {MochtroidAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
