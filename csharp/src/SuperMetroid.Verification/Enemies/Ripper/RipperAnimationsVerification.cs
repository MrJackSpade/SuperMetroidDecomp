using static SuperMetroid.Verification.Enemies.Ripper.RipperNativeListAddresses;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Ripper;

/// <summary>#1290: the Ripper family's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class RipperAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyRipperAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, RipperAnimations.Programs,
            RipperFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Ripper programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
