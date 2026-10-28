using static SuperMetroid.Verification.Enemies.Polyp.PolypNativeListAddresses;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>#641: Polyp's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class PolypAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPolypAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, PolypAnimations.Programs,
            PolypFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Polyp programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
