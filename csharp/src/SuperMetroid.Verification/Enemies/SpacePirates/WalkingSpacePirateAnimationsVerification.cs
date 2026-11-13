using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.SpacePirates.WalkingSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the walking Space Pirates' typed animations (#1600), each segment matched to its native bank-$B2 list and each animation to the lists' control flow.</summary>
internal static class WalkingSpacePirateAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyWalkingSpacePirateAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            WalkingSpacePirateAnimations.Segments, WalkingSpacePirateAnimations.StartingAt, WalkingSpacePirateFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<WalkingSpacePirateCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    instruction.Check(cue, at);
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'V' => unchecked((ushort)(cue switch { WalkingSpacePirateCue.FireLaserLeft fireLaserLeft => fireLaserLeft.LaserHeight, WalkingSpacePirateCue.FireLaserRight fireLaserRight => fireLaserRight.LaserHeight, _ => throw new InvalidOperationException($"{at}: {cue} carries no LaserHeight.") })),
                        'E' => SpacePiratesNativeWords.WalkingSpacePirateFunctions.Word(AssertIs<WalkingSpacePirateCue.SetFunction>(cue, $"{at}: cue").Function),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Walking Space Pirate animations: {Entries.Length} segments ({beats} beats) and {WalkingSpacePirateAnimations.StartingAt.Count} animations match bank $B2.");
    }
}
