using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#1290: the escaping Etecoons' typed programs, each matched step by step to its native bank-$B3 list.</summary>
internal static class EscapeEtecoonAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEscapeEtecoonAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, EscapeEtecoonAnimations.Programs,
            EscapeEtecoonFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyProgramOracle.EntryOf(Entries, cue.Target),
                    'S' => unchecked((ushort)cue.Offset),
                    'E' => (ushort)cue.PreInstruction,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  escape Etecoon programs: {Entries.Length} programs ({steps} steps) match bank $B3.");
    }
}
