using static SuperMetroid.Verification.Enemies.Sbug.SbugNativeListAddresses;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Sbug;

/// <summary>#641: Sbug's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class SbugAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySbugAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, SbugAnimations.Programs,
            SbugFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Sbug programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
