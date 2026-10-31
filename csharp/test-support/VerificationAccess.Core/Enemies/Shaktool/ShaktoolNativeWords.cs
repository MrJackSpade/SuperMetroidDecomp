namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Verification's native view of the Shaktool identities (#1412): the cartridge routine words that stand for each member.
/// Core's values are arbitrary; a native snapshot or ROM oracle decodes through this table. Two empty routines stand
/// for <see cref="ShaktoolFrameBehavior.Idle"/>, so checks decode a native word and compare identities.
/// </summary>
internal static class ShaktoolNativeWords
{
    /// <summary>The <see cref="ShaktoolFrameBehavior"/> each routine word stands for.</summary>
    private static readonly Dictionary<int, ShaktoolFrameBehavior> FrameBehaviorsByWord = new()
    {
        [0xdcaa] = ShaktoolFrameBehavior.Idle,
        [0xdcab] = ShaktoolFrameBehavior.Idle,
        [0xdcac] = ShaktoolFrameBehavior.OrbitPreviousSegment,
        [0xdcd7] = ShaktoolFrameBehavior.OrbitAndOrientCenter,
        [0xdd25] = ShaktoolFrameBehavior.DriveTailAndReverseAtWalls,
    };

    /// <summary>The pre-instruction the routine <paramref name="word"/> stands for, or throws naming <paramref name="meaning"/>.</summary>
    internal static ShaktoolFrameBehavior DecodeFrameBehavior(int word, string meaning) =>
        FrameBehaviorsByWord.TryGetValue(word, out ShaktoolFrameBehavior frameBehavior)
            ? frameBehavior
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({nameof(ShaktoolFrameBehavior)}).");
}
