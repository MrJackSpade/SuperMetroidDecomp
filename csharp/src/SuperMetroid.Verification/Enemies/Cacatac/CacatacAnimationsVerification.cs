using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Cacatac.CacatacNativeListAddresses;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#641, #1600: Cacatac's typed animations, each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class CacatacAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyCacatacAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            CacatacAnimations.Segments, CacatacAnimations.StartingAt, CacatacFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<CacatacCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'V' => CacatacNativeWords.SpikeDirections.Word(cue.Direction),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Cacatac animations: {Entries.Length} segments ({beats} beats) and {CacatacAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
