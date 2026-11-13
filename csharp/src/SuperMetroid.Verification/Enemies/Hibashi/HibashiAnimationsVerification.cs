using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Hibashi.HibashiNativeListAddresses;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>#641, #1600: the Hibashi's typed animations, each segment matched to its native bank-$A6 list and each animation to the lists' control flow.</summary>
internal static class HibashiAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyHibashiAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            HibashiAnimations.Segments, HibashiAnimations.StartingAt, HibashiFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<HibashiCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(instruction.Value, cue.Value, $"{at}: value");
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'V' => cue.Value,
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Hibashi animations: {Entries.Length} segments ({beats} beats) and {HibashiAnimations.StartingAt.Count} animations match bank $A6.");
    }
}
