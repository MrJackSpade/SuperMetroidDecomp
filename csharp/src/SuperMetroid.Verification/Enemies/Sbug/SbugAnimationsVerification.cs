using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Sbug.SbugNativeListAddresses;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Sbug;

/// <summary>#641: Sbug's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class SbugAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifySbugAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            SbugAnimations.Segments, SbugAnimations.StartingAt, SbugFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Sbug animations: {Entries.Length} segments ({beats} beats) and {SbugAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
