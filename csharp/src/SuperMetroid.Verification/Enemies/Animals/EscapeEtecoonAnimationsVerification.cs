using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#1290: the escaping Etecoons' typed programs, each matched step by step to its native bank-$B3 list.</summary>
internal static class EscapeEtecoonAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEscapeEtecoonAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, EscapeEtecoonAnimations.Programs,
            EscapeEtecoonFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyProgramOracle.EntryOf(Entries, AssertIs<EscapeEtecoonCue.GoToIfAcidHigh>(cue, $"{at}: cue").Target),
                    'S' => unchecked((ushort)AssertIs<EscapeEtecoonCue.Move>(cue, $"{at}: cue").Offset),
                    'E' => (ushort)AssertIs<EscapeEtecoonCue.SetPreInstruction>(cue, $"{at}: cue").PreInstruction,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  escape Etecoon programs: {Entries.Length} programs ({steps} steps) match bank $B3.");
    }
}
