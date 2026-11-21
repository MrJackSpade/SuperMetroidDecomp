using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlayCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteOverlaySegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite tracking overlay's throw and pillar-cap animations (#641, #2554). Each frame shows a
/// <see cref="EnemyFrameId"/> and each cue is a <see cref="MagdolliteOverlayCue"/> with the values it acts on, which only the
/// overlay carries out.
/// </summary>
internal static class MagdolliteOverlayAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteOverlaySegment, EnemyFrameId, MagdolliteOverlayCue> Segments { get; } = new(
        new Dictionary<MagdolliteOverlaySegment, AnimationBeat<EnemyFrameId, MagdolliteOverlayCue>[]>
    {
        [P.LeftThrow] =
        [
            Cue(new MagdolliteOverlayCue.PlaySoundIfOnScreen(MagdolliteSoundDefinitions.Throw)),
            Cue(new MagdolliteOverlayCue.Signal(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRearingFacingLeft, 26),
            Show(EnemyFrameId.MagdolliteLeaningFacingLeft, 8),
            Cue(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(-8, -4))),
        ],
        [P.LeftLavaFling] =
        [
            Show(EnemyFrameId.MagdolliteReachingFacingLeft, 5),
            Cue(new MagdolliteOverlayCue.Signal(K.SpawnLava)),
        ],
        [P.LeftThrowFinish] =
        [
            Cue(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(-8, -4))),
            Show(EnemyFrameId.MagdolliteThrowingFacingLeft, 5),
            Cue(new MagdolliteOverlayCue.Signal(K.FinishThrow)),
        ],
        [P.RightThrow] =
        [
            Cue(new MagdolliteOverlayCue.PlaySoundIfOnScreen(MagdolliteSoundDefinitions.Throw)),
            Cue(new MagdolliteOverlayCue.Signal(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRearingFacingRight, 26),
            Show(EnemyFrameId.MagdolliteLeaningFacingRight, 8),
            Cue(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(8, -4))),
        ],
        [P.RightLavaFling] =
        [
            Show(EnemyFrameId.MagdolliteReachingFacingRight, 5),
            Cue(new MagdolliteOverlayCue.Signal(K.SpawnLava)),
        ],
        [P.RightThrowFinish] =
        [
            Cue(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(8, -8))),
            Show(EnemyFrameId.MagdolliteThrowingFacingRight, 5),
            Cue(new MagdolliteOverlayCue.Signal(K.FinishThrow)),
        ],
        [P.PillarCap] =
        [
            Hold(EnemyFrameId.MagdollitePillarCap),
        ],
    });

    /// <summary>The gobs of lava a throw flings, one a play of its reaching hand's fling (#2439).</summary>
    private const int LavaFlings = 3;

    /// <summary>
    /// The animation that starts at each segment the overlay can start or restart at (#2337): a throw rears, flings
    /// <see cref="LavaFlings"/> gobs of lava and finishes (#2439), ending on its finishing cue, which moves the overlay on;
    /// the pillar cap is a still picture.
    /// </summary>
    private static Dictionary<MagdolliteOverlaySegment, Animation<MagdolliteOverlaySegment>> StartingAt { get; } = new Dictionary<MagdolliteOverlaySegment, Animation<MagdolliteOverlaySegment>>
    {
        [P.LeftThrow] = new([Segments.Whole(P.LeftThrow), Segments.Repeated(P.LeftLavaFling, LavaFlings), Segments.Whole(P.LeftThrowFinish)], AnimationEnd.Holds),
        [P.RightThrow] = new([Segments.Whole(P.RightThrow), Segments.Repeated(P.RightLavaFling, LavaFlings), Segments.Whole(P.RightThrowFinish)], AnimationEnd.Holds),
        [P.PillarCap] = new([Segments.Whole(P.PillarCap)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteOverlaySegment> StartingWith(MagdolliteOverlaySegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteOverlayCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteOverlayCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteOverlayCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MagdolliteOverlayCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteOverlayCue> Cue(MagdolliteOverlayCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteOverlayCue>.Event(cue);
}
