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
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        // Each walk routine loads the instruction pointer with its walk's list and returns (#2167): LDY #list, RTL.
        foreach ((ushort routine, WorkRobotSegment walk) in WalkRoutines)
        {
            int address = (int)new SnesAddress(Bank, routine);
            AssertEqual((byte)0xa0, cartridge.ReadCartridgeByte(address), $"walk routine ${routine:X4} loads the instruction pointer");
            AssertEqual(NativeEnemyAnimationOracle.EntryOf(Entries, walk), cartridge.ReadCartridgeWord(address + 1), $"walk routine ${routine:X4} goes on to {walk}");
            AssertEqual((byte)0x6b, cartridge.ReadCartridgeByte(address + 3), $"walk routine ${routine:X4} returns at once");
        }
        int beats = NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, Entries,
            WorkRobotAnimations.Segments, WorkRobotAnimations.StartingAt, WorkRobotFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<WorkRobotCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Cue, cue, $"{at}: cue");
                    AssertEqual(0, operands.Length, $"{at}: operands");
                },
                GoesTo: word => WalkRoutines.TryGetValue(word, out WorkRobotSegment walk) ? NativeEnemyAnimationOracle.EntryOf(Entries, walk) : null),
            // Each recoil only runs on from the shots that fire its laser (#2166).
            onlyRunOnInto: new HashSet<WorkRobotSegment> { WorkRobotSegment.FacingLeftLaserShotRecoil, WorkRobotSegment.FacingRightLaserShotRecoil });
        Console.WriteLine($"  Work Robot animations: {Entries.Length} segments ({beats} beats) and {WorkRobotAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
