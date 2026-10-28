using static SuperMetroid.Verification.Enemies.Powamp.PowampNativeListAddresses;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#641: Powamp's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class PowampAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPowampAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, PowampAnimations.Programs,
            PowampFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Powamp programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
