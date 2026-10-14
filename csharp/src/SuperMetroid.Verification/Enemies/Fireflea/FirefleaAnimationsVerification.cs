using static SuperMetroid.Verification.Enemies.Fireflea.FirefleaNativeListAddresses;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>#641: Fireflea's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class FirefleaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFirefleaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, FirefleaAnimations.Programs,
            FirefleaFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Fireflea programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
