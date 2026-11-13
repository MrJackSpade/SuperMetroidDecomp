using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Atomic.AtomicNativeListAddresses;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Atomic;

/// <summary>#641, #1600: Atomic's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class AtomicAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyAtomicAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            AtomicAnimations.Segments, AtomicAnimations.StartingAt, AtomicFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Atomic animations: {Entries.Length} segments ({beats} beats) and {AtomicAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
