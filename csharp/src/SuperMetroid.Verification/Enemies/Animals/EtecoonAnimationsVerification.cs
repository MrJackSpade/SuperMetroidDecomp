using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.EtecoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#641, #1600: the Etecoon's typed animations, each segment matched to its native bank-$A7 list and each animation to the lists' control flow.</summary>
internal static class EtecoonAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyEtecoonAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            EtecoonAnimations.Segments, EtecoonAnimations.StartingAt, EtecoonFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Etecoon animations: {Entries.Length} segments ({beats} beats) and {EtecoonAnimations.StartingAt.Count} animations match bank $A7.");
    }
}
