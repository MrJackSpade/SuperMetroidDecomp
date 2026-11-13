using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.WreckedShip.WreckedShipGhostNativeListAddresses;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.WreckedShip;

/// <summary>#641: the Wrecked Ship ghost's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class WreckedShipGhostAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyWreckedShipGhostAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            WreckedShipGhostAnimations.Segments, WreckedShipGhostAnimations.StartingAt, WreckedShipGhostFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Wrecked Ship ghost animations: {Entries.Length} segments ({beats} beats) and {WreckedShipGhostAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
