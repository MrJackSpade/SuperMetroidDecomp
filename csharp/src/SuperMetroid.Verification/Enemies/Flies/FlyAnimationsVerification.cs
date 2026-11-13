using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Flies.FlyNativeListAddresses;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Enemies.Flies;

/// <summary>#641, #1600: the Mellow, Mella and Memu typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class FlyAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyFlyAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            FlyAnimations.Segments, FlyAnimations.StartingAt, FlyFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  fly-family animations: {Entries.Length} segments ({beats} beats) and {FlyAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
