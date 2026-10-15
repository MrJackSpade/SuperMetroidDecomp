using static SuperMetroid.Verification.Enemies.Stoke.StokeNativeListAddresses;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#641: Stoke's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class StokeAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyStokeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, StokeAnimations.Programs,
            StokeFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => cue.Value,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Stoke programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
