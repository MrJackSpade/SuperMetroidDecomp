using static SuperMetroid.Verification.Enemies.SpacePirates.WallSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the wall Space Pirates' typed programs, each matched step by step to its native bank-$B2 list.</summary>
internal static class WallSpacePirateAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWallSpacePirateAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, WallSpacePirateAnimations.Programs,
            WallSpacePirateFrameDefinitions.Spritemap, new NativeEnemyCueRules<WallSpacePirateCue>(
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => unchecked((ushort)cue.ClimbPixels),
                    'E' => (ushort)cue.Function,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            },
            cue => cue.Kind is WallSpacePirateCueKind.ChooseLeftWallDirection or WallSpacePirateCueKind.ChooseRightWallDirection));
        Console.WriteLine($"  Wall Space Pirate programs: {Entries.Length} programs ({steps} steps) match bank $B2.");
    }
}
