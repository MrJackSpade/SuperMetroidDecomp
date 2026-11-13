using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Verification's native view of the Chozo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChozoNativeWords
{
    /// <summary>The routine word naming each <see cref="ChozoStatueFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<ChozoStatueFrameBehavior> ChozoStatueFrameBehaviors = new(new Dictionary<ChozoStatueFrameBehavior, ushort>
    {
        [ChozoStatueFrameBehavior.Idle] = 0xe7a6,
        [ChozoStatueFrameBehavior.Cleared] = 0x807b,
        [ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger] = 0xe445,
        [ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger] = 0xe7ae,
        [ChozoStatueFrameBehavior.Walking] = 0xe7da,
    });

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
