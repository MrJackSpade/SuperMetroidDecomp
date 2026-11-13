using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotNativeListAddresses;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>#641: the Work Robot's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class WorkRobotAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyWorkRobotAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt, WorkRobotFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<WorkRobotCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                }));
        Console.WriteLine($"  Work Robot animations: {Entries.Length} segments ({beats} beats) and {WorkRobotAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
