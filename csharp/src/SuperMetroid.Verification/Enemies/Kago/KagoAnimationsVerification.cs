using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Kago.KagoNativeListAddresses;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#641, #1600: Kago's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class KagoAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyKagoAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            KagoAnimations.Segments, KagoAnimations.StartingAt, KagoFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Kago animations: {Entries.Length} segments ({beats} beats) and {KagoAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
