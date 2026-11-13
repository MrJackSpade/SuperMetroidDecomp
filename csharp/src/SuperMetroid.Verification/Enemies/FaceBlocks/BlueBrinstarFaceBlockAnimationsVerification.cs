using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.FaceBlocks.BlueBrinstarFaceBlockNativeListAddresses;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.FaceBlocks;

/// <summary>#641, #1600: the Blue Brinstar face block's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class BlueBrinstarFaceBlockAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyBlueBrinstarFaceBlockAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            BlueBrinstarFaceBlockAnimations.Segments, BlueBrinstarFaceBlockAnimations.StartingAt, BlueBrinstarFaceBlockFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Blue Brinstar face block animations: {Entries.Length} segments ({beats} beats) and {BlueBrinstarFaceBlockAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
