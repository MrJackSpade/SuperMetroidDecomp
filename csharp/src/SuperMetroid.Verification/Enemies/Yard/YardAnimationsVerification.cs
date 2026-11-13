using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Yard.YardNativeListAddresses;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Yard.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>#641: the Yard's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class YardAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyYardAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            YardAnimations.Segments, YardAnimations.StartingAt, YardFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<YardCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    instruction.Check(cue, at);
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual(operands[index], instruction.Operands[index] switch
                    {
                        'D' => checked((ushort)SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.NumberOf(AssertIs<YardCue.SetDirection>(cue, $"{at}: cue").Direction)),
                        'S' => unchecked((ushort)AssertIs<YardCue.MoveBy>(cue, $"{at}: cue").Offset.X),
                        'Y' => unchecked((ushort)AssertIs<YardCue.MoveBy>(cue, $"{at}: cue").Offset.Y),
                        'N' => AssertIs<YardCue.SetHidingAnimation>(cue, $"{at}: cue").OptionalTarget is { } optional ? NativeEnemyAnimationOracle.EntryOf(Entries, optional) : NullTarget,
                        'E' => YardNativeWords.YardMovements.Word(AssertIs<YardCue.SetMovement>(cue, $"{at}: cue").Movement),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Yard animations: {Entries.Length} segments ({beats} beats) and {YardAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
