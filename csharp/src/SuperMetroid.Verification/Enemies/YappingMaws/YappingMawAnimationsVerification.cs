using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawNativeListAddresses;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#641: the Yapping Maw's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class YappingMawAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyYappingMawAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, YappingMawAnimations.Programs,
            YappingMawFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Yapping Maw programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
