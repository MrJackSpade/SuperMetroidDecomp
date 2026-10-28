using static SuperMetroid.Verification.Enemies.Hibashi.HibashiNativeListAddresses;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>#641: the Hibashi's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class HibashiAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyHibashiAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, HibashiAnimations.Programs,
            HibashiFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(instruction.Value, cue.Value, $"{at}: value");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => cue.Value,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Hibashi programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
