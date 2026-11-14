using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>
/// Botwoon's presentation boundary (#641): the installed artwork identity of every typed head and
/// projectile frame. Gameplay animates <see cref="BotwoonHeadFrame"/> and <see cref="BotwoonProjectileFrame"/>
/// values; only this catalog knows the frames that draw them.
/// </summary>
internal static class BotwoonVisualDefinitions
{

    /// <summary>
    /// The installed frame each head pose shows: the visible frames are named in the physical order of
    /// <c>Frames</c>, closed then open; the hidden head shows the shared empty frame.
    /// </summary>
    internal static EnemyFrameTable<BotwoonHeadFrame> HeadFrames { get; } = new()
    {
        { BotwoonHeadFrame.Hidden, EnemyFrameId.Empty },
        { BotwoonHeadFrame.Closed(CompassDirection.UpLeft), EnemyFrameId.BotwoonHeadClosedUpLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.Left), EnemyFrameId.BotwoonHeadClosedLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.DownLeft), EnemyFrameId.BotwoonHeadClosedDownLeft },
        { BotwoonHeadFrame.Closed(CompassDirection.Down), EnemyFrameId.BotwoonHeadClosedDown },
        { BotwoonHeadFrame.Closed(CompassDirection.DownRight), EnemyFrameId.BotwoonHeadClosedDownRight },
        { BotwoonHeadFrame.Closed(CompassDirection.Right), EnemyFrameId.BotwoonHeadClosedRight },
        { BotwoonHeadFrame.Closed(CompassDirection.UpRight), EnemyFrameId.BotwoonHeadClosedUpRight },
        { BotwoonHeadFrame.Closed(CompassDirection.Up), EnemyFrameId.BotwoonHeadClosedUp },
        { BotwoonHeadFrame.Open(CompassDirection.UpLeft), EnemyFrameId.BotwoonHeadOpenUpLeft },
        { BotwoonHeadFrame.Open(CompassDirection.Left), EnemyFrameId.BotwoonHeadOpenLeft },
        { BotwoonHeadFrame.Open(CompassDirection.DownLeft), EnemyFrameId.BotwoonHeadOpenDownLeft },
        { BotwoonHeadFrame.Open(CompassDirection.Down), EnemyFrameId.BotwoonHeadOpenDown },
        { BotwoonHeadFrame.Open(CompassDirection.DownRight), EnemyFrameId.BotwoonHeadOpenDownRight },
        { BotwoonHeadFrame.Open(CompassDirection.Right), EnemyFrameId.BotwoonHeadOpenRight },
        { BotwoonHeadFrame.Open(CompassDirection.UpRight), EnemyFrameId.BotwoonHeadOpenUpRight },
        { BotwoonHeadFrame.Open(CompassDirection.Up), EnemyFrameId.BotwoonHeadOpenUp },
    };

    /// <summary>
    /// The projectile frame each drawn frame of the family shows (#1869): every body link and tail cap by facing, the
    /// hidden segment and the spit's five frames.
    /// </summary>
    internal static IReadOnlyDictionary<BotwoonProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<BotwoonProjectileFrame, EnemyProjectileFrameId>
        {
            [BotwoonProjectileFrame.Body(CompassDirection.Up, 0)] = EnemyProjectileFrameId.BotwoonBodyUpFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.Up, 1)] = EnemyProjectileFrameId.BotwoonBodyUpSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.Up, 2)] = EnemyProjectileFrameId.BotwoonBodyUpThird,
            [BotwoonProjectileFrame.Body(CompassDirection.Up, 3)] = EnemyProjectileFrameId.BotwoonBodyUpFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.UpRight, 0)] = EnemyProjectileFrameId.BotwoonBodyUpRightFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.UpRight, 1)] = EnemyProjectileFrameId.BotwoonBodyUpRightSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.UpRight, 2)] = EnemyProjectileFrameId.BotwoonBodyUpRightThird,
            [BotwoonProjectileFrame.Body(CompassDirection.UpRight, 3)] = EnemyProjectileFrameId.BotwoonBodyUpRightFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.Right, 0)] = EnemyProjectileFrameId.BotwoonBodyRightFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.Right, 1)] = EnemyProjectileFrameId.BotwoonBodyRightSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.Right, 2)] = EnemyProjectileFrameId.BotwoonBodyRightThird,
            [BotwoonProjectileFrame.Body(CompassDirection.Right, 3)] = EnemyProjectileFrameId.BotwoonBodyRightFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.DownRight, 0)] = EnemyProjectileFrameId.BotwoonBodyDownRightFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.DownRight, 1)] = EnemyProjectileFrameId.BotwoonBodyDownRightSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.DownRight, 2)] = EnemyProjectileFrameId.BotwoonBodyDownRightThird,
            [BotwoonProjectileFrame.Body(CompassDirection.DownRight, 3)] = EnemyProjectileFrameId.BotwoonBodyDownRightFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.Down, 0)] = EnemyProjectileFrameId.BotwoonBodyDownFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.Down, 1)] = EnemyProjectileFrameId.BotwoonBodyDownSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.Down, 2)] = EnemyProjectileFrameId.BotwoonBodyDownThird,
            [BotwoonProjectileFrame.Body(CompassDirection.Down, 3)] = EnemyProjectileFrameId.BotwoonBodyDownFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.DownLeft, 0)] = EnemyProjectileFrameId.BotwoonBodyDownLeftFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.DownLeft, 1)] = EnemyProjectileFrameId.BotwoonBodyDownLeftSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.DownLeft, 2)] = EnemyProjectileFrameId.BotwoonBodyDownLeftThird,
            [BotwoonProjectileFrame.Body(CompassDirection.DownLeft, 3)] = EnemyProjectileFrameId.BotwoonBodyDownLeftFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.Left, 0)] = EnemyProjectileFrameId.BotwoonBodyLeftFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.Left, 1)] = EnemyProjectileFrameId.BotwoonBodyLeftSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.Left, 2)] = EnemyProjectileFrameId.BotwoonBodyLeftThird,
            [BotwoonProjectileFrame.Body(CompassDirection.Left, 3)] = EnemyProjectileFrameId.BotwoonBodyLeftFourth,
            [BotwoonProjectileFrame.Body(CompassDirection.UpLeft, 0)] = EnemyProjectileFrameId.BotwoonBodyUpLeftFirst,
            [BotwoonProjectileFrame.Body(CompassDirection.UpLeft, 1)] = EnemyProjectileFrameId.BotwoonBodyUpLeftSecond,
            [BotwoonProjectileFrame.Body(CompassDirection.UpLeft, 2)] = EnemyProjectileFrameId.BotwoonBodyUpLeftThird,
            [BotwoonProjectileFrame.Body(CompassDirection.UpLeft, 3)] = EnemyProjectileFrameId.BotwoonBodyUpLeftFourth,
            [BotwoonProjectileFrame.Tail(CompassDirection.Up)] = EnemyProjectileFrameId.BotwoonTailUp,
            [BotwoonProjectileFrame.Tail(CompassDirection.UpRight)] = EnemyProjectileFrameId.BotwoonTailUpRight,
            [BotwoonProjectileFrame.Tail(CompassDirection.Right)] = EnemyProjectileFrameId.BotwoonTailRight,
            [BotwoonProjectileFrame.Tail(CompassDirection.DownRight)] = EnemyProjectileFrameId.BotwoonTailDownRight,
            [BotwoonProjectileFrame.Tail(CompassDirection.Down)] = EnemyProjectileFrameId.BotwoonTailDown,
            [BotwoonProjectileFrame.Tail(CompassDirection.DownLeft)] = EnemyProjectileFrameId.BotwoonTailDownLeft,
            [BotwoonProjectileFrame.Tail(CompassDirection.Left)] = EnemyProjectileFrameId.BotwoonTailLeft,
            [BotwoonProjectileFrame.Tail(CompassDirection.UpLeft)] = EnemyProjectileFrameId.BotwoonTailUpLeft,
            [BotwoonProjectileFrame.Hidden] = EnemyProjectileFrameId.BotwoonHiddenUp,
            [BotwoonProjectileFrame.Spit(0)] = EnemyProjectileFrameId.BotwoonSpitUpFirst,
            [BotwoonProjectileFrame.Spit(1)] = EnemyProjectileFrameId.BotwoonSpitUpSecond,
            [BotwoonProjectileFrame.Spit(2)] = EnemyProjectileFrameId.BotwoonSpitUpThird,
            [BotwoonProjectileFrame.Spit(3)] = EnemyProjectileFrameId.BotwoonSpitUpFourth,
            [BotwoonProjectileFrame.Spit(4)] = EnemyProjectileFrameId.BotwoonSpitUpFifth,
        };
}
