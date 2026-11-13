using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Areas.Tourian.TourianEntranceStatueNativeListAddresses;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>#641, #1600: the Tourian entrance statue's typed animations, each segment matched to its native bank-$AA list.</summary>
internal static class TourianEntranceStatueAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyTourianEntranceStatueAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, TourianEntranceStatueAnimations.Segments,
            TourianEntranceStatueAnimations.StartingAt,
            frame => EnemyFrameCartridgeLookupTooling.Word(TourianEntranceStatueFrameDefinitions.ShownFrames.Of(frame)),
            new NativeEnemyCueRules<NoAnimationCue>(word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Tourian entrance statue animations: {Entries.Length} segments ({beats} beats) match bank $AA.");
    }
}
