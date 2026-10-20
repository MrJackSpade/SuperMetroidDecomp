using static SuperMetroid.Verification.Enemies.Bull.BullNativeListAddresses;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>#641: Bull's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class BullAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyBullAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, BullAnimations.Programs,
            BullFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Bull programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
