using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Stoke.StokeNativeListAddresses;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#641: Stoke's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class StokeAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyStokeAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            StokeAnimations.Segments, StokeAnimations.StartingAt, StokeFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<StokeCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'V' => (ushort)cue.FireballDirection,
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Stoke animations: {Entries.Length} segments ({beats} beats) and {StokeAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
