using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaNativeListAddresses;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>#641, #1600: Fireflea's typed animations, each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class FirefleaAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyFirefleaAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            FirefleaAnimations.Segments, FirefleaAnimations.StartingAt, FirefleaFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Fireflea animations: {Entries.Length} segments ({beats} beats) and {FirefleaAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
