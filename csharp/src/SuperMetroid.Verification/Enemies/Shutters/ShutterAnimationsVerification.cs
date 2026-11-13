using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Shutters.ShutterNativeListAddresses;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>#641: the shutters' and Kamer's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class ShutterAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyShutterAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ShutterAnimations.Segments, ShutterAnimations.StartingAt, ShutterFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  shutter animations: {Entries.Length} segments ({beats} beats) and {ShutterAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
