using static SuperMetroid.Verification.Enemies.Animals.EscapeDachoraNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#1290: the escaping Dachora's typed programs, each matched step by step to its native bank-$B3 list.</summary>
internal static class EscapeDachoraAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEscapeDachoraAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, EscapeDachoraAnimations.Programs,
            EscapeDachoraFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyProgramOracle.EntryOf(Entries, cue.Target),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  escape Dachora programs: {Entries.Length} programs ({steps} steps) match bank $B3.");
    }
}
