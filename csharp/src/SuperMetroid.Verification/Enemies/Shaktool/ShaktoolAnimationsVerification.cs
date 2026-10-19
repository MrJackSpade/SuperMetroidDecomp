using static SuperMetroid.Verification.Enemies.Shaktool.ShaktoolNativeListAddresses;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>#641: the Shaktool's typed programs, each matched step by step to its native bank-$AA list.</summary>
internal static class ShaktoolAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyShaktoolAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ShaktoolAnimations.Programs,
            ShaktoolFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Shaktool programs: {Entries.Length} programs ({steps} steps) match bank $AA.");
    }
}
