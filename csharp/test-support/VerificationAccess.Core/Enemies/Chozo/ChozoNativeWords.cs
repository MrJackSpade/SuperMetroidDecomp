namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Verification's native view of the Chozo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChozoNativeWords
{
    /// <summary>The routine word of the Lower Norfair statue's wait for Samus at its hand.</summary>
    internal const ushort LowerNorfairHandWaitWord = 0xe445;

    /// <summary>The routine word of the Wrecked Ship statue's wait for Samus at its hand and Phantoon's defeat.</summary>
    internal const ushort WreckedShipHandWaitWord = 0xe7ae;

    /// <summary>The routine word of the wait for Samus at its hand of the Wrecked Ship statue, or of Lower Norfair's.</summary>
    internal static ushort HandWaitWord(bool wreckedShip) => wreckedShip ? WreckedShipHandWaitWord : LowerNorfairHandWaitWord;

    /// <summary>The routine word of the frame routine the statue's initializer installs, which returns at once (#1803).</summary>
    internal const ushort InitializerFrameRoutineWord = 0xe7a6;

    /// <summary>
    /// The routine words of the frame routines that return at once which the statue's activation installs (#1803, #2476): the
    /// cleared one and the one installed while the Wrecked Ship statue walks. A statue running one has started its
    /// activation, whose phase says it from the update that starts it, so the oracle reads their installs as nothing.
    /// </summary>
    internal static readonly HashSet<ushort> ActivatedFrameRoutineWords = [0x807b, 0xe7da];

    /// <summary>
    /// The phase a recorded routine word of the statue's site names: the site's wait, nothing yet or the initializer's
    /// routine before the idle animation's first beat installs the wait, or a routine the activation installs.
    /// </summary>
    /// <exception cref="InvalidDataException">The word is none of the routines the statue's site runs.</exception>
    internal static ChozoStatuePhase PhaseAt(ushort word, bool wreckedShip) =>
        word == HandWaitWord(wreckedShip) ? ChozoStatuePhase.Placed.AfterWaitStarts
        : word is 0 or InitializerFrameRoutineWord ? ChozoStatuePhase.Placed
        : ActivatedFrameRoutineWords.Contains(word) ? ChozoStatuePhase.Activated
        : throw new InvalidDataException($"Recorded Chozo statue routine ${word:X4} is not one its site runs.");

    /// <summary>The cartridge's carry-motion records per facing: sixteen facing left, then their sixteen mirrors facing right.</summary>
    internal const int CarryRecordsPerFacing = 16;

    /// <summary>The poses the first records of each facing hold, in record order: taking Samus, then holding her.</summary>
    private static readonly ChozoCarryPose[] HoldingPoses =
        [ChozoCarryPose.Reaching, ChozoCarryPose.Closing, ChozoCarryPose.Gripping, ChozoCarryPose.Holding];

    /// <summary>The stride's steps, in the order the next records of each facing repeat them twice.</summary>
    private static readonly ChozoCarryPose[] StridePoses =
        [ChozoCarryPose.StrideWeightShift, ChozoCarryPose.StrideLift, ChozoCarryPose.StrideSwing, ChozoCarryPose.StridePlant];

    /// <summary>
    /// The carry step cartridge carry-motion record <paramref name="record"/> holds: within each facing's sixteen, the
    /// four poses that take and hold Samus, the stride's four steps twice over, then four still records no cue names.
    /// </summary>
    /// <exception cref="InvalidDataException">The record is outside the table or one of the still records no cue names.</exception>
    internal static ChozoCarryStep CarryStep(int record)
    {
        if (record is < 0 or >= 2 * CarryRecordsPerFacing)
            throw new InvalidDataException($"Chozo carry record {record} is outside its {2 * CarryRecordsPerFacing} records.");
        HorizontalDirection facing = record < CarryRecordsPerFacing ? HorizontalDirection.Left : HorizontalDirection.Right;
        int local = record % CarryRecordsPerFacing;
        int strideRecords = 2 * StridePoses.Length;
        return local < HoldingPoses.Length ? new(facing, HoldingPoses[local])
            : local < HoldingPoses.Length + strideRecords ? new(facing, StridePoses[(local - HoldingPoses.Length) % StridePoses.Length])
            : throw new InvalidDataException($"Chozo carry record {record} is a still record no cue names.");
    }

    /// <summary>Every carry-motion record that holds a carry step, in record order.</summary>
    internal static IEnumerable<int> StepRecords { get; } =
        [.. Enumerable.Range(0, 2 * CarryRecordsPerFacing).Where(record => record % CarryRecordsPerFacing < HoldingPoses.Length + 2 * StridePoses.Length)];
}
