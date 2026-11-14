using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Zoa.ZoaSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ZoaCue"/> naming what it does.
/// </summary>
internal static class ZoaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ZoaSegment, EnemyFrameId, ZoaCue> Segments { get; } = new(
        new Dictionary<ZoaSegment, AnimationBeat<EnemyFrameId, ZoaCue>[]>
    {
        [P.FacingLeftShooting] =
        [
            Cue(new(ZoaSpeedDefinitions.SlowLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingLeftShootingFirst, 64),
            Cue(new(ZoaSpeedDefinitions.MiddleLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingLeftShootingSecond, 8),
            Cue(new(ZoaSpeedDefinitions.FastLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingLeftShootingThird, 48),
        ],
        [P.FacingLeftRising] =
        [
            Show(EnemyFrameId.ZoaFacingLeftRisingFirst, 4),
            Show(EnemyFrameId.ZoaFacingLeftRisingSecond, 4),
            Show(EnemyFrameId.ZoaFacingLeftRisingThird, 4),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(ZoaSpeedDefinitions.SlowLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingRightShootingFirst, 64),
            Cue(new(ZoaSpeedDefinitions.MiddleLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingRightShootingSecond, 8),
            Cue(new(ZoaSpeedDefinitions.FastLaunchSpeed)),
            Show(EnemyFrameId.ZoaFacingRightShootingThird, 48),
        ],
        [P.FacingRightRising] =
        [
            Show(EnemyFrameId.ZoaFacingRightRisingFirst, 4),
            Show(EnemyFrameId.ZoaFacingRightRisingSecond, 4),
            Show(EnemyFrameId.ZoaFacingRightRisingThird, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ZoaSegment, Animation<ZoaSegment>> StartingAt { get; } = new Dictionary<ZoaSegment, Animation<ZoaSegment>>
    {
        [P.FacingLeftShooting] = new([new(P.FacingLeftShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRising] = new([new(P.FacingLeftRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightShooting] = new([new(P.FacingRightShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRising] = new([new(P.FacingRightRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ZoaSegment> StartingWith(ZoaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ZoaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Cue(ZoaCue cue) => new AnimationBeat<EnemyFrameId, ZoaCue>.Event(cue);
}
