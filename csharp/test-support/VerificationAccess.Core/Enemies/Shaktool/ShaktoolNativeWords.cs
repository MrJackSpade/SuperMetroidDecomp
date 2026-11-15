namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Verification's native view of the Shaktool identities (#1412): the cartridge routine words that stand for each member.
/// Core's values are arbitrary; a native snapshot or ROM oracle decodes through this table. Two empty routines stand
/// for <see cref="ShaktoolSegmentMotion.Idle"/>, so checks decode a native word and compare identities.
/// </summary>
internal static class ShaktoolNativeWords
{
    /// <summary>The <see cref="ShaktoolSegmentMotion"/> each routine word stands for.</summary>
    private static readonly Dictionary<int, ShaktoolSegmentMotion> MotionsByWord = new()
    {
        [0xdcaa] = ShaktoolSegmentMotion.Idle,
        [0xdcab] = ShaktoolSegmentMotion.Idle,
        [0xdcac] = ShaktoolSegmentMotion.OrbitPreviousSegment,
        [0xdcd7] = ShaktoolSegmentMotion.OrbitAndOrientCenter,
        [0xdd25] = ShaktoolSegmentMotion.DriveTailAndReverseAtWalls,
    };

    /// <summary>The pre-instruction the routine <paramref name="word"/> stands for, or throws naming <paramref name="meaning"/>.</summary>
    internal static ShaktoolSegmentMotion DecodeMotion(int word, string meaning) =>
        MotionsByWord.TryGetValue(word, out ShaktoolSegmentMotion motion)
            ? motion
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({nameof(ShaktoolSegmentMotion)}).");
}
