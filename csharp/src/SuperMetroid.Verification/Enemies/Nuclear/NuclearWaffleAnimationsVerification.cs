using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleNativeListAddresses;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#641: Nuclear Waffle's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class NuclearWaffleAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNuclearWaffleAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, NuclearWaffleAnimations.Programs,
            NuclearWaffleFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Nuclear Waffle programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
