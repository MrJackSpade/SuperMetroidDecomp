using static SuperMetroid.Verification.Enemies.Zoa.ZoaNativeListAddresses;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies.Zoa.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>#641: Zoa's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class ZoaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyZoaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ZoaAnimations.Programs,
            ZoaFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Zoa programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
