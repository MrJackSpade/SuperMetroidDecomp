using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Skultera.SkulteraNativeListAddresses;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>#641: Skultera's typed animations (#1600), each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class SkulteraAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifySkulteraAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            SkulteraAnimations.Segments, SkulteraAnimations.StartingAt, SkulteraFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<SkulteraCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : OmittedInstructions.Contains(word) ? 0 : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                Omitted: OmittedInstructions.Contains));
        Console.WriteLine($"  Skultera animations: {Entries.Length} segments ({beats} beats) and {SkulteraAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
