using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Bull.BullNativeListAddresses;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>#641, #1600: Bull's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class BullAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyBullAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            BullAnimations.Segments, BullAnimations.StartingAt, BullFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Bull animations: {Entries.Length} segments ({beats} beats) and {BullAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
