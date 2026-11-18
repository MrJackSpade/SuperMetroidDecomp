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
            Show(EnemyFrameId.ZoaDartingFacingLeftTailTucked, 64),
            Cue(new(ZoaSpeedDefinitions.MiddleLaunchSpeed)),
            Show(EnemyFrameId.ZoaDartingFacingLeftTailKicking, 8),
            Cue(new(ZoaSpeedDefinitions.FastLaunchSpeed)),
            Show(EnemyFrameId.ZoaDartingFacingLeftTailExtended, 48),
        ],
        [P.FacingLeftRising] =
        [
            Show(EnemyFrameId.ZoaSwimmingFacingLeftLegsIn, 4),
            Show(EnemyFrameId.ZoaSwimmingFacingLeftLegsPaddling, 4),
            Show(EnemyFrameId.ZoaSwimmingFacingLeftLegsOut, 4),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(ZoaSpeedDefinitions.SlowLaunchSpeed)),
            Show(EnemyFrameId.ZoaDartingFacingRightTailTucked, 64),
            Cue(new(ZoaSpeedDefinitions.MiddleLaunchSpeed)),
            Show(EnemyFrameId.ZoaDartingFacingRightTailKicking, 8),
            Cue(new(ZoaSpeedDefinitions.FastLaunchSpeed)),
            Show(EnemyFrameId.ZoaDartingFacingRightTailExtended, 48),
        ],
        [P.FacingRightRising] =
        [
            Show(EnemyFrameId.ZoaSwimmingFacingRightLegsIn, 4),
            Show(EnemyFrameId.ZoaSwimmingFacingRightLegsPaddling, 4),
            Show(EnemyFrameId.ZoaSwimmingFacingRightLegsOut, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ZoaSegment, Animation<ZoaSegment>> StartingAt { get; } = new Dictionary<ZoaSegment, Animation<ZoaSegment>>
    {
        [P.FacingLeftShooting] = new([Segments.Whole(P.FacingLeftShooting)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRising] = new([Segments.Whole(P.FacingLeftRising)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightShooting] = new([Segments.Whole(P.FacingRightShooting)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRising] = new([Segments.Whole(P.FacingRightRising)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ZoaSegment> StartingWith(ZoaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ZoaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Cue(ZoaCue cue) => new AnimationBeat<EnemyFrameId, ZoaCue>.Event(cue);
}
