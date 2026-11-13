using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.TestSupport.MovieReplay.PipeBugNativeListAddresses;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>#641, #1600: the Pipe Bugs' typed animations, each segment matched beat by beat to its native bank-$B3 list.</summary>
internal static class PipeBugAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyPipeBugAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, PipeBugAnimations.Segments,
            PipeBugAnimations.StartingAt, PipeBugFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Pipe Bug animations: {Entries.Length} segments ({beats} beats) match bank $B3.");
    }
}
