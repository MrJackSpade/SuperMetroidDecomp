using static SuperMetroid.Verification.Enemies.Boulder.BoulderNativeListAddresses;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>#641: the Boulder's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class BoulderAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyBoulderAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, BoulderAnimations.Programs,
            BoulderFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Boulder programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
