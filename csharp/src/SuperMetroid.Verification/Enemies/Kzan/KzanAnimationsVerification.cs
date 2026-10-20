using static SuperMetroid.Verification.Enemies.Kzan.KzanNativeListAddresses;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Kzan;

/// <summary>#641: Kzan's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class KzanAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKzanAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, KzanAnimations.Programs,
            KzanFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Kzan programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
