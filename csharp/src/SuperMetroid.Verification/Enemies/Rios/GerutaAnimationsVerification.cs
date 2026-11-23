using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Rios.GerutaNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Geruta's typed animations (#1600), each segment matched to its native bank-$A2 list and each animation to the lists' control flow.</summary>
internal static class GerutaAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyGerutaAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, Entries,
            GerutaAnimations.Segments, GerutaAnimations.StartingAt, GerutaFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<GerutaCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                // Only the flame cues and the setup place the flames (#2512), so placing them again restates the place.
                Restates: cue => cue is GerutaCue.PlaceFlames));
        Console.WriteLine($"  Norfair Rio animations: {Entries.Length} segments ({beats} beats) and {GerutaAnimations.StartingAt.Count} animations match bank $A2.");
    }
}
