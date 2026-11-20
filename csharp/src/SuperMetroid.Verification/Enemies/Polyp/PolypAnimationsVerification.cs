using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Polyp.PolypNativeListAddresses;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>#641: Polyp's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class PolypAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyPolypAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            PolypAnimations.Segments, PolypAnimations.StartingAt, PolypFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Polyp animations: {Entries.Length} segments ({beats} beats) and {PolypAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
