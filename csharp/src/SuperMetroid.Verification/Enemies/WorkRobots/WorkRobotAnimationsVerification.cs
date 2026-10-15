using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotNativeListAddresses;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>#641: the Work Robot's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class WorkRobotAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWorkRobotAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, WorkRobotAnimations.Programs,
            WorkRobotFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Work Robot programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
