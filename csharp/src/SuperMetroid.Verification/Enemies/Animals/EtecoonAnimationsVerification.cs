using static SuperMetroid.Verification.Enemies.Animals.EtecoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#641: the Etecoon's typed programs, each matched step by step to its native bank-$A7 list.</summary>
internal static class EtecoonAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEtecoonAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, EtecoonAnimations.Programs,
            EtecoonFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Etecoon programs: {Entries.Length} programs ({steps} steps) match bank $A7.");
    }
}
