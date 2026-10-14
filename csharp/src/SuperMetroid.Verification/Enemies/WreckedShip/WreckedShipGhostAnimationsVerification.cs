using static SuperMetroid.Verification.Enemies.WreckedShip.WreckedShipGhostNativeListAddresses;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.WreckedShip;

/// <summary>#641: the Wrecked Ship ghost's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class WreckedShipGhostAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWreckedShipGhostAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, WreckedShipGhostAnimations.Programs,
            WreckedShipGhostFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Wrecked Ship ghost programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
