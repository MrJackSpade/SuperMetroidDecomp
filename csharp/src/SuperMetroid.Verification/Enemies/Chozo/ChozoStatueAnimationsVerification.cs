using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueNativeListAddresses;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>#641, #1600: the Chozo statue's typed animations, each segment matched to its native bank-$AA list and each animation to the lists' control flow.</summary>
internal static class ChozoStatueAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyChozoStatueAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            ChozoStatueAnimations.Segments, ChozoStatueAnimations.StartingAt, ChozoStatueFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<ChozoStatueCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    instruction.Check(cue, at);
                    for (int index = 0; index < operands.Length; index++)
                    {
                        // The native carry operand is a record's byte offset: the record names the cue's carry step.
                        if (instruction.Operands[index] == 'R')
                        {
                            AssertEqual(ChozoNativeWords.CarryStep(operands[index] / 2),
                                AssertIs<ChozoStatueCue.Move>(cue, $"{at}: cue").Step, $"{at}: operand {index} carry step");
                            continue;
                        }

                        if (instruction.Operands[index] == 'E' && cue is ChozoStatueCue.Signal)
                        {
                            AssertTrue(ChozoNativeWords.NoFrameBehaviorWords.Contains(operands[index]), $"{at}: operand {index} is a frame routine that returns at once");
                            continue;
                        }

                        AssertEqual(operands[index], instruction.Operands[index] switch
                        {
                            // The other native operands are the footstep's signed X word and the frame behaviour's word.
                            'F' => unchecked((ushort)AssertIs<ChozoStatueCue.Footstep>(cue, $"{at}: cue").Offset.X),
                            'E' => ChozoNativeWords.ChozoStatueFrameBehaviors.Word(AssertIs<ChozoStatueCue.SetFrameBehavior>(cue, $"{at}: cue").FrameBehavior),
                            _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                        }, $"{at}: operand {index}");
                    }
                }));
        Console.WriteLine($"  Chozo statue animations: {Entries.Length} segments ({beats} beats) and {ChozoStatueAnimations.StartingAt.Count} animations match bank $AA.");
    }
}
