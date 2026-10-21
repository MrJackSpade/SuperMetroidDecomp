using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueNativeListAddresses;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>#641: the Chozo statue's typed programs, each matched step by step to its native bank-$AA list.</summary>
internal static class ChozoStatueAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyChozoStatueAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ChozoStatueAnimations.Programs,
            ChozoStatueFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    // The native operands are the footstep's signed X word and the carry record's byte offset.
                    'F' => unchecked((ushort)AssertIs<ChozoStatueCue.Footstep>(cue, $"{at}: cue").Offset.X),
                    'R' => checked((ushort)(AssertIs<ChozoStatueCue.Move>(cue, $"{at}: cue").CarryRecord * 2)),
                    'E' => (ushort)AssertIs<ChozoStatueCue.SetPreInstruction>(cue, $"{at}: cue").PreInstruction,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Chozo statue programs: {Entries.Length} programs ({steps} steps) match bank $AA.");
    }
}
