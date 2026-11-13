using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.MorphBallEye.MorphBallEyeNativeListAddresses;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>#641: the Morph Ball eye's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class MorphBallEyeAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMorphBallEyeAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MorphBallEyeAnimations.Segments, MorphBallEyeAnimations.StartingAt, MorphBallEyeFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Morph Ball eye animations: {Entries.Length} segments ({beats} beats) and {MorphBallEyeAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
