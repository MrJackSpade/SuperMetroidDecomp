using static SuperMetroid.Verification.Enemies.Yard.YardNativeListAddresses;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Yard.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>#641: the Yard's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class YardAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyYardAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, YardAnimations.Programs,
            YardFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'D' => checked((ushort)cue.Direction),
                    'S' => unchecked((ushort)cue.Offset),
                    'Y' => unchecked((ushort)cue.OffsetY),
                    'N' => cue.OptionalTarget is { } optional ? NativeEnemyProgramOracle.EntryOf(Entries, optional) : NullTarget,
                    'E' => (ushort)cue.MovementFunction,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Yard programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
