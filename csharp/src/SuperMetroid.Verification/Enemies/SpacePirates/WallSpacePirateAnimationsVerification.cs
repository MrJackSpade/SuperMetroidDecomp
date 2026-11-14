using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.SpacePirates.WallSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the wall Space Pirates' typed animations (#1600), each segment matched to its native bank-$B2 list and each animation to the lists' control flow.</summary>
internal static class WallSpacePirateAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyWallSpacePirateAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            WallSpacePirateAnimations.Segments, WallSpacePirateAnimations.StartingAt, WallSpacePirateFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<WallSpacePirateCue>(
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => unchecked((ushort)(cue switch { WallSpacePirateCue.ClimbLeftWall climbLeftWall => climbLeftWall.ClimbPixels, WallSpacePirateCue.ClimbRightWall climbRightWall => climbRightWall.ClimbPixels, _ => throw new InvalidOperationException($"{at}: {cue} carries no ClimbPixels.") })),
                    'E' => NamingWord(SpacePiratesNativeWords.WallSpacePirateActions.WordsByIdentity[AssertIs<WallSpacePirateCue.SetAction>(cue, $"{at}: cue").Action], operands[index]),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            },
            cue => cue is WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.ChooseLeftWallDirection } or WallSpacePirateCue.Signal { Kind: WallSpacePirateCueKind.ChooseRightWallDirection }));
        Console.WriteLine($"  Wall Space Pirate animations: {Entries.Length} segments ({beats} beats) and {WallSpacePirateAnimations.StartingAt.Count} animations match bank $B2.");
    }

    /// <summary><paramref name="word"/> when it is one of <paramref name="words"/>, otherwise the first of them, which the comparison then reports.</summary>
    private static ushort NamingWord(ushort[] words, ushort word) => words.Contains(word) ? word : words[0];
}
