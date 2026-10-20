using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoNativeListAddresses;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>#641: Dead Torizo's typed programs, each matched step by step to its native bank-$A9 list.</summary>
internal static class DeadTorizoAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDeadTorizoAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, DeadTorizoAnimations.Programs,
            DeadTorizoFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Dead Torizo programs: {Entries.Length} programs ({steps} steps) match bank $A9.");
    }
}
