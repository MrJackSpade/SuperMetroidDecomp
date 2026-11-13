using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.LavaJumpers.NorfairLavaJumperNativeListAddresses;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.LavaJumpers;

/// <summary>#641: the Norfair lava jumper's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class NorfairLavaJumperAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyNorfairLavaJumperAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            NorfairLavaJumperAnimations.Segments, NorfairLavaJumperAnimations.StartingAt, NorfairLavaJumperFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NorfairLavaJumperCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Norfair lava jumper animations: {Entries.Length} segments ({beats} beats) and {NorfairLavaJumperAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
