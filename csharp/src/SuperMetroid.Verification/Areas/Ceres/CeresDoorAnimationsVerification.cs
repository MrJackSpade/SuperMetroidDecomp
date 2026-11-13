using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorNativeListAddresses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>#1290: the Ceres door's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class CeresDoorAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyCeresDoorAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CeresDoorAnimations.Segments, CeresDoorAnimations.StartingAt,
            CeresDoorFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<CeresDoorCue>(word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null, (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyAnimationOracle.EntryOf(Entries, cue.Target),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            }));
        Console.WriteLine($"  Ceres door animations: {Entries.Length} segments ({beats} beats) match bank $A6.");
    }
}
