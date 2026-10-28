using static SuperMetroid.Verification.Enemies.Cacatac.CacatacNativeListAddresses;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#641: Cacatac's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class CacatacAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCacatacAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CacatacAnimations.Programs,
            CacatacFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => (ushort)cue.Direction,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Cacatac programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
