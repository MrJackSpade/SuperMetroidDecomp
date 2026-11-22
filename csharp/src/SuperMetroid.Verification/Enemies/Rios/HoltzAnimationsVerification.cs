using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Rios.HoltzNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Holtz's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class HoltzAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyHoltzAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, Entries,
            HoltzAnimations.Segments, HoltzAnimations.StartingAt, HoltzFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<HoltzCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }),
            onlyRunOnInto: new HashSet<HoltzSegment> { HoltzSegment.IdleWingbeat });
        Console.WriteLine($"  Lower Norfair Rio animations: {Entries.Length} segments ({beats} beats) and {HoltzAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
