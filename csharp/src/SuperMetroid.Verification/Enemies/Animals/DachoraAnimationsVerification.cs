using static SuperMetroid.Verification.Enemies.Animals.DachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#641: the Dachora's typed programs, each matched step by step to its native bank-$A7 list.</summary>
internal static class DachoraAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDachoraAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, DachoraAnimations.Programs,
            DachoraFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Dachora programs: {Entries.Length} programs ({steps} steps) match bank $A7.");
    }
}
