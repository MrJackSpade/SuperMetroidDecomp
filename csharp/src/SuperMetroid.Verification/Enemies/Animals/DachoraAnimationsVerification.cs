using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.DachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#641, #1600: the Dachora's typed animations, each segment matched to its native bank-$A7 list and each animation to the lists' control flow.</summary>
internal static class DachoraAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyDachoraAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            DachoraAnimations.Segments, DachoraAnimations.StartingAt, DachoraFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Dachora animations: {Entries.Length} segments ({beats} beats) and {DachoraAnimations.StartingAt.Count} animations match bank $A7.");
    }
}
