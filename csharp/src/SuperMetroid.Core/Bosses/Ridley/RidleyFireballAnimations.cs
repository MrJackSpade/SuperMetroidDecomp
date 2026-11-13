using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.Ridley.RidleyFireballCue;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyFireballFrameKind;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyFireballSegment;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn animations (#641, #1600), in place of their lists.</summary>
internal static class RidleyFireballAnimations
{
    /// <summary>Poses in the flight loop.</summary>
    internal const int FlightPoseCount = 4;
    /// <summary>Poses in each afterburn run.</summary>
    internal const int AfterburnPoseCount = 5;

    /// <summary>Each launch pose's dwell.</summary>
    private const int LaunchDuration = 4;
    /// <summary>Each flight pose's dwell.</summary>
    private const int FlightDuration = 2;
    /// <summary>Each afterburn pose's dwell.</summary>
    private const int AfterburnDuration = 5;

    /// <summary>Every fireball and afterburn segment's beats.</summary>
    internal static AnimationSegments<RidleyFireballSegment, RidleyFireballFrame, RidleyFireballCue> Segments { get; } = new(
        new Dictionary<RidleyFireballSegment, AnimationBeat<RidleyFireballFrame, RidleyFireballCue>[]>
    {
        [P.Fireball] =
        [
            Cue(C.ClearFrameBehavior),
            Show(new(K.Launch, 0), LaunchDuration),
            Cue(C.BeginFlight),
            Show(new(K.Launch, 1), LaunchDuration),
        ],
        [P.FireballLoop] = [.. Enumerable.Range(0, FlightPoseCount).Select(phase => Show(new(K.Flight, phase), FlightDuration))],
        [P.AfterburnFinal] = Afterburn(K.AfterburnFinal, spawn: null),
        [P.HorizontalCenter] = Afterburn(K.HorizontalCenter, C.SpawnHorizontalAfterburns),
        [P.VerticalCenter] = Afterburn(K.VerticalCenter, C.SpawnVerticalAfterburns),
        [P.DirectionalAfterburn] = Afterburn(K.Directional, C.SpawnNextAfterburn),
    });

    /// <summary>
    /// The animation that starts at each segment: the launch runs on into the flight loop, and each afterburn run holds
    /// after its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt { get; } =
        new Dictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>>
        {
            [P.Fireball] = new([Whole(P.Fireball), Whole(P.FireballLoop)], AnimationEnd.LoopsFrom(1)),
            [P.FireballLoop] = new([Whole(P.FireballLoop)], AnimationEnd.LoopsFrom(0)),
            [P.AfterburnFinal] = new([Whole(P.AfterburnFinal)], AnimationEnd.Holds),
            [P.HorizontalCenter] = new([Whole(P.HorizontalCenter)], AnimationEnd.Holds),
            [P.VerticalCenter] = new([Whole(P.VerticalCenter)], AnimationEnd.Holds),
            [P.DirectionalAfterburn] = new([Whole(P.DirectionalAfterburn)], AnimationEnd.Holds),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RidleyFireballSegment> StartingWith(RidleyFireballSegment segment) => StartingAt[segment];

    /// <summary>A phase playing every beat of <paramref name="segment"/> once.</summary>
    private static AnimationPhase<RidleyFireballSegment> Whole(RidleyFireballSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>
    /// An afterburn run: it stops moving, shows five poses (raising <paramref name="spawn"/> after the first, when it has
    /// one) and deletes itself.
    /// </summary>
    private static AnimationBeat<RidleyFireballFrame, RidleyFireballCue>[] Afterburn(K kind, C? spawn)
    {
        var beats = new List<AnimationBeat<RidleyFireballFrame, RidleyFireballCue>> { Cue(C.ClearFrameBehavior) };
        for (int phase = 0; phase < AfterburnPoseCount; phase++)
        {
            beats.Add(Show(new(kind, phase), AfterburnDuration));
            if (phase == 0 && spawn is { } cue)
                beats.Add(Cue(cue));
        }
        beats.Add(Cue(C.Delete));
        return [.. beats];
    }

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<RidleyFireballFrame, RidleyFireballCue> Show(RidleyFireballFrame frame, int duration) =>
        new AnimationBeat<RidleyFireballFrame, RidleyFireballCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<RidleyFireballFrame, RidleyFireballCue> Cue(RidleyFireballCue cue) =>
        new AnimationBeat<RidleyFireballFrame, RidleyFireballCue>.Event(cue);
}
