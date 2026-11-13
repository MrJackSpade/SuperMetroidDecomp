using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipNativeListAddresses;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>#1290, #1600: the gunship's hull and entrance pad typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class GunshipAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyGunshipAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            GunshipAnimations.Segments, GunshipAnimations.StartingAt, GunshipFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  gunship animations: {Entries.Length} segments ({beats} beats) and {GunshipAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
