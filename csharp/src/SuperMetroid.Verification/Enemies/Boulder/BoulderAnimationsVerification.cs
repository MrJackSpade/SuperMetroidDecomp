using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Boulder.BoulderNativeListAddresses;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>#641, #1600: the Boulder's typed animations, each segment matched to its native bank-$A6 list and each animation to the lists' control flow.</summary>
internal static class BoulderAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyBoulderAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            BoulderAnimations.Segments, BoulderAnimations.StartingAt, BoulderFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Boulder animations: {Entries.Length} segments ({beats} beats) and {BoulderAnimations.StartingAt.Count} animations match bank $A6.");
    }
}
