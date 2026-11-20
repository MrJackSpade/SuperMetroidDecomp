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
                    // A signal command raises its signal; the spike's spawn, which names no signal, spits the spike its operand aims.
                    AssertEqual(instruction.Kind, (cue as CacatacCue.Signal)?.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'V' => CacatacNativeWords.SpikeDirections.Word(AssertIs<CacatacCue.SpawnSpike>(cue, $"{at}: spike cue").Direction),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }),
            onlyRunOnInto: new HashSet<CacatacSegment> { CacatacSegment.OnCeilingIdlePoses });
        Console.WriteLine($"  Cacatac animations: {Entries.Length} segments ({beats} beats) and {CacatacAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
