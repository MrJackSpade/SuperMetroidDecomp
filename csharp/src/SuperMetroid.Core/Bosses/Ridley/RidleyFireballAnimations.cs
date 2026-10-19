using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.Ridley.RidleyFireballCue;
using K = SuperMetroid.Core.Bosses.Ridley.RidleyFireballFrameKind;
using P = SuperMetroid.Core.Bosses.Ridley.RidleyFireballProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn programs (#641), in place of their bank-$86 lists.</summary>
internal static class RidleyFireballAnimations
{
    /// <summary>Poses in the flight loop.</summary>
    internal const int FlightPoseCount = 4;
    /// <summary>Poses in each afterburn run.</summary>
    internal const int AfterburnPoseCount = 5;

    /// <summary>Each launch pose's dwell ($86:9554/955C).</summary>
    private const int LaunchDuration = 4;
    /// <summary>Each flight pose's dwell ($86:9560..956C).</summary>
    private const int FlightDuration = 2;
    /// <summary>Each afterburn pose's dwell ($86:9576..961A).</summary>
    private const int AfterburnDuration = 5;

    /// <summary>Every fireball and afterburn program.</summary>
    internal static AnimationProgramSet<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> Programs { get; } = new(
        new Dictionary<RidleyFireballProgram, AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>[]>
    {
        [P.Fireball] =
        [
            Cue(C.ClearPreInstruction),
            Show(new(K.Launch, 0), LaunchDuration),
            Cue(C.BeginFlight),
            Show(new(K.Launch, 1), LaunchDuration),
            FallInto(P.FireballLoop),
        ],
        [P.FireballLoop] =
        [
            .. Enumerable.Range(0, FlightPoseCount).Select(phase => Show(new(K.Flight, phase), FlightDuration)),
            GoTo(P.FireballLoop),
        ],
        [P.AfterburnFinal] = Afterburn(K.AfterburnFinal, spawn: null),
        [P.HorizontalCenter] = Afterburn(K.HorizontalCenter, C.SpawnHorizontalAfterburns),
        [P.VerticalCenter] = Afterburn(K.VerticalCenter, C.SpawnVerticalAfterburns),
        [P.DirectionalAfterburn] = Afterburn(K.Directional, C.SpawnNextAfterburn),
    });

    /// <summary>
    /// An afterburn run: it stops moving, shows five poses (publishing <paramref name="spawn"/> after the first, when
    /// it has one) and deletes itself.
    /// </summary>
    private static AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>[] Afterburn(K kind, C? spawn)
    {
        var steps = new List<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>> { Cue(C.ClearPreInstruction) };
        for (int phase = 0; phase < AfterburnPoseCount; phase++)
        {
            steps.Add(Show(new(kind, phase), AfterburnDuration));
            if (phase == 0 && spawn is { } cue)
                steps.Add(Cue(cue));
        }
        steps.Add(Cue(C.Delete));
        steps.Add(AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.Hold);
        return [.. steps];
    }

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> Show(RidleyFireballFrame frame, int duration) =>
        AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> Cue(RidleyFireballCue cue) =>
        AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> GoTo(RidleyFireballProgram program) =>
        AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> FallInto(RidleyFireballProgram program) =>
        AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.FallInto(program);
}
