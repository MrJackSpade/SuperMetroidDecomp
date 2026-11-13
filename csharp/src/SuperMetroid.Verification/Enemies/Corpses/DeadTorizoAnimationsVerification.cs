using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoNativeListAddresses;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>#641, #1600: Dead Torizo's typed animations, each segment matched to its native bank-$A9 list and each animation to the lists' control flow.</summary>
internal static class DeadTorizoAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyDeadTorizoAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            DeadTorizoAnimations.Segments, DeadTorizoAnimations.StartingAt, DeadTorizoFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Dead Torizo animations: {Entries.Length} segments ({beats} beats) and {DeadTorizoAnimations.StartingAt.Count} animations match bank $A9.");
    }
}
