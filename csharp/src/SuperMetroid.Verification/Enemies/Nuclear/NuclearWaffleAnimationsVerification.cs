using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleNativeListAddresses;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#641: Nuclear Waffle's typed animations (#1600), each segment matched to its native bank-$A6 list and each animation to the lists' control flow.</summary>
internal static class NuclearWaffleAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyNuclearWaffleAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            NuclearWaffleAnimations.Segments, NuclearWaffleAnimations.StartingAt, NuclearWaffleFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Nuclear Waffle animations: {Entries.Length} segments ({beats} beats) and {NuclearWaffleAnimations.StartingAt.Count} animations match bank $A6.");
    }
}
