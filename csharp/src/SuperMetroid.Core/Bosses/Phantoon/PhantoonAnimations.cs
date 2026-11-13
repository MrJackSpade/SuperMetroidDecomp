using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartCue;
using F = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartFrameKind;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartProgram;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed body, eye, tentacle and mouth animations (#641, #1600).</summary>
internal static class PhantoonAnimations
{
    /// <summary>The eyelid's dwell on each of its two moving poses.</summary>
    private const int EyelidFrameDuration = 10;
    /// <summary>The tentacles' dwell on each of their four sway poses.</summary>
    private const int TentacleFrameDuration = 8;
    /// <summary>The mouth's dwell on each of its two opening poses.</summary>
    private const int MouthFrameDuration = 5;

    /// <summary>Every body, eye, tentacle and mouth segment.</summary>
    internal static AnimationSegments<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> Segments { get; } = new(
        new Dictionary<PhantoonPartProgram, AnimationBeat<PhantoonPartFrame, PhantoonPartCue>[]>
    {
        [P.InvulnerableBody] = Still(new(F.InvulnerableBody, 0)),
        [P.FullHitboxBody] = Still(new(F.FullHitboxBody, 0)),
        [P.EyeHitboxBody] = Still(new(F.EyeHitboxBody, 0)),
        [P.EyeOpen] =
        [
            Show(new(F.OpeningEye, 0), EyelidFrameDuration),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
            Show(new(F.OpeningEye, 2), 1),
            Cue(C.PlayMaterializationSound),
            Cue(C.BeginEyeTracking),
        ],
        [P.EyeClosed] = Still(new(F.ClosedEye, 0)),
        [P.EyeCloseAndPickNewPattern] =
        [
            Show(new(F.OpeningEye, 2), 1),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
            Cue(C.PickNewPattern),
        ],
        [P.EyeClose] =
        [
            Show(new(F.OpeningEye, 2), 1),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
        ],
        [P.EyeballCentered] =
        [
            Show(new(F.CenteredEye, 0), 1),
            Cue(C.PlayMaterializationSound),
        ],
        [P.EyeLookingUp] = Still(new(F.Gaze, 0)),
        [P.EyeLookingUpRight] = Still(new(F.Gaze, 1)),
        [P.EyeLookingRight] = Still(new(F.Gaze, 2)),
        [P.EyeLookingDownRight] = Still(new(F.Gaze, 3)),
        [P.EyeLookingDown] = Still(new(F.Gaze, 4)),
        [P.EyeLookingDownLeft] = Still(new(F.Gaze, 5)),
        [P.EyeLookingLeft] = Still(new(F.Gaze, 6)),
        [P.EyeLookingUpLeft] = Still(new(F.Gaze, 7)),
        [P.Tentacles] =
        [
            Show(new(F.Tentacle, 0), TentacleFrameDuration),
            Show(new(F.Tentacle, 1), TentacleFrameDuration),
            Show(new(F.Tentacle, 2), TentacleFrameDuration),
            Show(new(F.Tentacle, 1), TentacleFrameDuration),
        ],
        [P.MouthSpawnFlame] =
        [
            Show(new(F.Mouth, 2), MouthFrameDuration),
            Show(new(F.Mouth, 1), MouthFrameDuration),
            Cue(C.SpawnCasualFlame),
        ],
        [P.MouthInitial] = Still(new(F.Mouth, 0)),
    });

    /// <summary>
    /// The animation that starts at each segment: a still pose and the opening or centring eye hold their last frame,
    /// the closing eye closes and holds closed, the tentacles sway for good, and the flame-spitting mouth runs on into its
    /// initial pose and holds it.
    /// </summary>
    internal static IReadOnlyDictionary<PhantoonPartProgram, Animation<PhantoonPartProgram>> StartingAt { get; } = new Dictionary<PhantoonPartProgram, Animation<PhantoonPartProgram>>
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
        [P.Tentacles] = new([Whole(P.Tentacles)], AnimationEnd.LoopsFrom(0)),
        [P.MouthSpawnFlame] = HeldAfter(P.MouthSpawnFlame, P.MouthInitial),
        [P.MouthInitial] = Held(P.MouthInitial),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonPartProgram> StartingWith(PhantoonPartProgram segment) => StartingAt[segment];

    /// <summary>Plays <paramref name="segment"/> once and holds its last frame.</summary>
    private static Animation<PhantoonPartProgram> Held(PhantoonPartProgram segment) => new([Whole(segment)], AnimationEnd.Holds);

    /// <summary>Plays <paramref name="first"/>, then <paramref name="then"/>, and holds the last frame.</summary>
    private static Animation<PhantoonPartProgram> HeldAfter(PhantoonPartProgram first, PhantoonPartProgram then) =>
        new([Whole(first), Whole(then)], AnimationEnd.Holds);

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<PhantoonPartProgram> Whole(PhantoonPartProgram segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows one frame for an update; its animation holds it, as the script's sleep does.</summary>
    private static AnimationBeat<PhantoonPartFrame, PhantoonPartCue>[] Still(PhantoonPartFrame frame) => [Show(frame, 1)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<PhantoonPartFrame, PhantoonPartCue> Show(PhantoonPartFrame frame, int duration) =>
        new AnimationBeat<PhantoonPartFrame, PhantoonPartCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<PhantoonPartFrame, PhantoonPartCue> Cue(PhantoonPartCue cue) =>
        new AnimationBeat<PhantoonPartFrame, PhantoonPartCue>.Event(cue);
}
