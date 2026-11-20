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
        IImportCartridgeSource cartridge = rom;
        int beats = NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, Entries,
            HibashiAnimations.Segments, HibashiAnimations.StartingAt, HibashiFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<HibashiCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length
                    : Array.IndexOf(RisingStepRoutines, word) >= 0 ? 0 : null,
                (word, operands, cue, at) =>
                {
                    AssertEqual(0, operands.Length, $"{at}: operands");
                    int step = Array.IndexOf(RisingStepRoutines, word);
                    if (step < 0)
                    {
                        AssertEqual(Instructions[word].Cue, cue, $"{at}: cue");
                        return;
                    }
                    // A rising cue's native routine is the step's own (#2337): the cue carries the step the native tables
                    // hold at the routine's place, and only the first restores the collision part's width.
                    AssertEqual(new HibashiCue.EruptionStep(new(
                            cartridge.ReadCartridgeWord(StepYOffsetTable + step * 2),
                            cartridge.ReadCartridgeWord(StepYRadiusTable + step * 2),
                            step == 0 ? HibashiStepWidth.Restored : HibashiStepWidth.Kept)),
                        cue, $"{at}: rising step {step}");
                }));
        Console.WriteLine($"  Hibashi animations: {Entries.Length} segments ({beats} beats) and {HibashiAnimations.StartingAt.Count} animations match bank $A6.");
    }
}
