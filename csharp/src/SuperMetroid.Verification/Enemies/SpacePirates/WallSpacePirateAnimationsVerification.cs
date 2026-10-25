using static SuperMetroid.Verification.Enemies.SpacePirates.WallSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the wall Space Pirates' typed programs, each matched step by step to its native bank-$B2 list.</summary>
internal static class WallSpacePirateAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWallSpacePirateAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, WallSpacePirateAnimations.Programs,
            WallSpacePirateFrameToolingDefinitions.Spritemap, new NativeEnemyCueRules<WallSpacePirateCue>(
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => unchecked((ushort)(cue switch { WallSpacePirateCue.ClimbLeftWall climbLeftWall => climbLeftWall.ClimbPixels, WallSpacePirateCue.ClimbRightWall climbRightWall => climbRightWall.ClimbPixels, _ => throw new InvalidOperationException($"{at}: {cue} carries no ClimbPixels.") })),
                    'E' => SpacePiratesNativeWords.WallSpacePirateFunctions.Word(AssertIs<WallSpacePirateCue.SetFunction>(cue, $"{at}: cue").Function),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            },
            cue => cue is WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.ChooseLeftWallDirection } or WallSpacePirateCue.Command { Kind: WallSpacePirateCueKind.ChooseRightWallDirection }));
        Console.WriteLine($"  Wall Space Pirate programs: {Entries.Length} programs ({steps} steps) match bank $B2.");
    }
}
