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
        [P.FacingLeftShooting] = Shooting(EnemyFrameId.ZoaDartingFacingLeftTailTucked, EnemyFrameId.ZoaDartingFacingLeftTailKicking, EnemyFrameId.ZoaDartingFacingLeftTailExtended),
        [P.FacingLeftRising] = Rising(EnemyFrameId.ZoaSwimmingFacingLeftLegsIn, EnemyFrameId.ZoaSwimmingFacingLeftLegsPaddling, EnemyFrameId.ZoaSwimmingFacingLeftLegsOut),
        [P.FacingRightShooting] = Shooting(EnemyFrameId.ZoaDartingFacingRightTailTucked, EnemyFrameId.ZoaDartingFacingRightTailKicking, EnemyFrameId.ZoaDartingFacingRightTailExtended),
        [P.FacingRightRising] = Rising(EnemyFrameId.ZoaSwimmingFacingRightLegsIn, EnemyFrameId.ZoaSwimmingFacingRightLegsPaddling, EnemyFrameId.ZoaSwimmingFacingRightLegsOut),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<ZoaSegment, Animation<ZoaSegment>> StartingAt { get; } = new Dictionary<ZoaSegment, Animation<ZoaSegment>>
    {
        [P.FacingLeftShooting] = Animation<ZoaSegment>.Looping([Segments.Whole(P.FacingLeftShooting)]),
        [P.FacingLeftRising] = Animation<ZoaSegment>.Looping([Segments.Whole(P.FacingLeftRising)]),
        [P.FacingRightShooting] = Animation<ZoaSegment>.Looping([Segments.Whole(P.FacingRightShooting)]),
        [P.FacingRightRising] = Animation<ZoaSegment>.Looping([Segments.Whole(P.FacingRightRising)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ZoaSegment> StartingWith(ZoaSegment segment) => StartingAt[segment];

    /// <summary>A Zoa shooting over a facing's poses, <paramref name="tailTucked"/>, <paramref name="tailKicking"/>, <paramref name="tailExtended"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue>[] Shooting(EnemyFrameId tailTucked, EnemyFrameId tailKicking, EnemyFrameId tailExtended) =>
    [
        Cue(new(ZoaSpeedDefinitions.SlowLaunchSpeed)),
        Show(tailTucked, 64),
        Cue(new(ZoaSpeedDefinitions.MiddleLaunchSpeed)),
        Show(tailKicking, 8),
        Cue(new(ZoaSpeedDefinitions.FastLaunchSpeed)),
        Show(tailExtended, 48),
    ];

    /// <summary>A Zoa rising over a facing's poses, <paramref name="legsIn"/>, <paramref name="legsPaddling"/>, <paramref name="legsOut"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue>[] Rising(EnemyFrameId legsIn, EnemyFrameId legsPaddling, EnemyFrameId legsOut) =>
    [
        Show(legsIn, 4),
        Show(legsPaddling, 4),
        Show(legsOut, 4),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ZoaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Cue(ZoaCue cue) => new AnimationBeat<EnemyFrameId, ZoaCue>.Event(cue);
}
