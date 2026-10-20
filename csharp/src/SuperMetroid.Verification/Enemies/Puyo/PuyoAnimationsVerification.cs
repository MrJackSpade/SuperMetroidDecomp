using static SuperMetroid.Verification.Enemies.Puyo.PuyoNativeListAddresses;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>#641: Puyo's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class PuyoAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPuyoAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, PuyoAnimations.Programs,
            PuyoFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Puyo programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
