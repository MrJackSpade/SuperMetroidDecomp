using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Animals.EscapeEtecoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>#1290, #1600: the escaping Etecoons' typed animations, each segment matched to its native bank-$B3 list and each animation to the lists' control flow.</summary>
internal static class EscapeEtecoonAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyEscapeEtecoonAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            EscapeEtecoonAnimations.Segments, EscapeEtecoonAnimations.StartingAt, EscapeEtecoonFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<EscapeEtecoonCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    instruction.Check(cue, at);
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'T' => NativeEnemyAnimationOracle.EntryOf(Entries, AssertIs<EscapeEtecoonCue.GoToIfAcidHigh>(cue, $"{at}: cue").Target),
                        'S' => unchecked((ushort)AssertIs<EscapeEtecoonCue.Move>(cue, $"{at}: cue").Offset),
                        'E' => AnimalsNativeWords.EscapeEtecoonFrameBehaviors.Word(AssertIs<EscapeEtecoonCue.SetFrameBehavior>(cue, $"{at}: cue").FrameBehavior),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  escape Etecoon animations: {Entries.Length} segments ({beats} beats) and {EscapeEtecoonAnimations.StartingAt.Count} animations match bank $B3.");
    }
}
