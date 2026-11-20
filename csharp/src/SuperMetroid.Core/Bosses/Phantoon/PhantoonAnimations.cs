using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartCue;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartSegment;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed body, eye, tentacle and mouth animations (#641, #1600), each beat naming the installed frame it shows (#1885).</summary>
internal static class PhantoonAnimations
{
    /// <summary>The eyelid's dwell on each of its two moving poses.</summary>
    private const int EyelidFrameDuration = 10;
    /// <summary>The tentacles' dwell on each of their four sway poses.</summary>
    private const int TentacleFrameDuration = 8;
    /// <summary>The mouth's dwell on each of its two opening poses.</summary>
    private const int MouthFrameDuration = 5;

    /// <summary>Every body, eye, tentacle and mouth segment.</summary>
    internal static AnimationSegments<PhantoonPartSegment, EnemyFrameId, PhantoonPartCue> Segments { get; } = new(
        new Dictionary<PhantoonPartSegment, AnimationBeat<EnemyFrameId, PhantoonPartCue>[]>
    {
        [P.InvulnerableBody] = Still(F.PhantoonInvulnerableBody),
        [P.FullHitboxBody] = Still(F.PhantoonFullHitboxBody),
        [P.EyeHitboxBody] = Still(F.PhantoonEyeHitboxBody),
        [P.EyeOpen] =
        [
            Show(F.PhantoonEyeClosed, EyelidFrameDuration),
            Show(F.PhantoonEyeHalfOpen, EyelidFrameDuration),
            Show(F.PhantoonEyeballCentered, 1),
            Cue(C.PlayMaterializationSound),
            Cue(C.BeginEyeTracking),
        ],
        [P.EyeClosed] = Still(F.PhantoonEyeClosed),
        [P.EyeCloseAndPickNewPattern] =
        [
            Show(F.PhantoonEyeballCentered, 1),
            Show(F.PhantoonEyeHalfOpen, EyelidFrameDuration),
            Cue(C.PickNewPattern),
        ],
        [P.EyeClose] =
        [
            Show(F.PhantoonEyeballCentered, 1),
            Show(F.PhantoonEyeHalfOpen, EyelidFrameDuration),
        ],
        [P.EyeballCentered] =
        [
            Show(F.PhantoonEyeballCentered, 1),
            Cue(C.PlayMaterializationSound),
        ],
        [P.EyeLookingUp] = Still(F.PhantoonEyeLookingUp),
        [P.EyeLookingUpRight] = Still(F.PhantoonEyeLookingUpRight),
        [P.EyeLookingRight] = Still(F.PhantoonEyeLookingRight),
        [P.EyeLookingDownRight] = Still(F.PhantoonEyeLookingDownRight),
        [P.EyeLookingDown] = Still(F.PhantoonEyeLookingDown),
        [P.EyeLookingDownLeft] = Still(F.PhantoonEyeLookingDownLeft),
        [P.EyeLookingLeft] = Still(F.PhantoonEyeLookingLeft),
        [P.EyeLookingUpLeft] = Still(F.PhantoonEyeLookingUpLeft),
        [P.Tentacles] =
        [
            Show(F.PhantoonTentaclesNarrow, TentacleFrameDuration),
            Show(F.PhantoonTentaclesSpreading, TentacleFrameDuration),
            Show(F.PhantoonTentaclesSpread, TentacleFrameDuration),
            Show(F.PhantoonTentaclesSpreading, TentacleFrameDuration),
        ],
        [P.MouthSpawnFlame] =
        [
            Show(F.PhantoonMouthNarrow, MouthFrameDuration),
            Show(F.PhantoonMouthHalfOpen, MouthFrameDuration),
            Cue(C.SpawnCasualFlame),
        ],
        [P.MouthInitial] = Still(F.PhantoonMouthOpen),
    });

    /// <summary>
    /// The animation that starts at each segment: a still pose and the opening or centring eye hold their last frame,
    /// the closing eye closes and holds closed, the tentacles sway for good, and the flame-spitting mouth runs on into its
    /// initial pose and holds it.
    /// </summary>
    private static Dictionary<PhantoonPartSegment, Animation<PhantoonPartSegment>> StartingAt { get; } = new Dictionary<PhantoonPartSegment, Animation<PhantoonPartSegment>>
    {
        [P.InvulnerableBody] = Held(P.InvulnerableBody),
        [P.FullHitboxBody] = Held(P.FullHitboxBody),
        [P.EyeHitboxBody] = Held(P.EyeHitboxBody),
        [P.EyeOpen] = Held(P.EyeOpen),
        [P.EyeClosed] = Held(P.EyeClosed),
        [P.EyeCloseAndPickNewPattern] = HeldAfter(P.EyeCloseAndPickNewPattern, P.EyeClosed),
        [P.EyeClose] = HeldAfter(P.EyeClose, P.EyeClosed),
        [P.EyeballCentered] = Held(P.EyeballCentered),
        [P.EyeLookingUp] = Held(P.EyeLookingUp),
        [P.EyeLookingUpRight] = Held(P.EyeLookingUpRight),
        [P.EyeLookingRight] = Held(P.EyeLookingRight),
        [P.EyeLookingDownRight] = Held(P.EyeLookingDownRight),
        [P.EyeLookingDown] = Held(P.EyeLookingDown),
        [P.EyeLookingDownLeft] = Held(P.EyeLookingDownLeft),
        [P.EyeLookingLeft] = Held(P.EyeLookingLeft),
        [P.EyeLookingUpLeft] = Held(P.EyeLookingUpLeft),
        [P.Tentacles] = Animation<PhantoonPartSegment>.Looping([Segments.Whole(P.Tentacles)]),
        [P.MouthSpawnFlame] = HeldAfter(P.MouthSpawnFlame, P.MouthInitial),
        [P.MouthInitial] = Held(P.MouthInitial),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonPartSegment> StartingWith(PhantoonPartSegment segment) => StartingAt[segment];

    /// <summary>Plays <paramref name="segment"/> once and holds its last frame.</summary>
    private static Animation<PhantoonPartSegment> Held(PhantoonPartSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Holds);

    /// <summary>Plays <paramref name="first"/>, then <paramref name="then"/>, and holds the last frame.</summary>
    private static Animation<PhantoonPartSegment> HeldAfter(PhantoonPartSegment first, PhantoonPartSegment then) =>
        new([Segments.Whole(first), Segments.Whole(then)], AnimationEnd.Holds);

    /// <summary>A still picture (#2250): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, PhantoonPartCue>[] Still(EnemyFrameId frame) =>
        [new AnimationBeat<EnemyFrameId, PhantoonPartCue>.HeldFrame(frame)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, PhantoonPartCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, PhantoonPartCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, PhantoonPartCue> Cue(PhantoonPartCue cue) =>
        new AnimationBeat<EnemyFrameId, PhantoonPartCue>.Event(cue);
}
