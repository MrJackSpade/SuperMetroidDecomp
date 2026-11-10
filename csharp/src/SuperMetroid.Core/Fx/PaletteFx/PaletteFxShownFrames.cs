using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The frames a set of colour effects' programs can show, each under the colour group its last cue named (#1512, #1598):
/// each program's frames and those of each program it jumps, falls or repeats into, or restarts at above the Crateria
/// lightning's height. The import pre-colours a shade for each; the ending names the shade family of each.
/// </summary>
internal static class PaletteFxShownFrames
{
    /// <summary>Every colour group and frame <paramref name="starts"/> can show, each once, ordered by group, program and frame.</summary>
    /// <exception cref="InvalidOperationException">A program holds a step of no defined kind.</exception>
    internal static ShownPaletteFxFrame[] From(IEnumerable<PaletteFxProgram> starts)
    {
        Ensure.NotNull(starts);
        var shown = new HashSet<ShownPaletteFxFrame>();
        foreach (PaletteFxProgram start in starts)
            Collect(start, shown);
        return [.. shown
            .OrderBy(entry => entry.Colours)
            .ThenBy(entry => entry.Frame.Animation)
            .ThenBy(entry => entry.Frame.Frame)];
    }

    /// <summary>The frames the initial programs of <paramref name="effects"/> can show, as <see cref="From"/> gives them.</summary>
    internal static ShownPaletteFxFrame[] ShownBy(IEnumerable<PaletteFxObject> effects) =>
        From(Ensure.NotNull(effects).Select(effect => RoomPaletteFxSelection.Get(effect).InitialProgram));

    /// <summary>Adds every colour group and frame <paramref name="start"/> can show to <paramref name="shown"/>.</summary>
    /// <exception cref="InvalidOperationException">A program holds a step of no defined kind.</exception>
    private static void Collect(PaletteFxProgram start, HashSet<ShownPaletteFxFrame> shown)
    {
        var visited = new HashSet<PaletteFxReachedProgram>();
        var pending = new Stack<PaletteFxReachedProgram>();
        pending.Push(new PaletteFxReachedProgram(start, null));
        while (pending.Count > 0)
        {
            PaletteFxReachedProgram reached = pending.Pop();
            if (!visited.Add(reached))
                continue;
            PaletteFxColours? colours = reached.Colours;
            foreach (AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(reached.Program))
            {
                switch (step)
                {
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetColours setColours }:
                        colours = setColours.Target;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.CueStep { Cue: PaletteFxCue.Watch { Condition: ColourEffectCondition.RestartsWhileSamusIsHigh high } }:
                        pending.Push(new PaletteFxReachedProgram(high.Restart, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.ShowStep show:
                        if (colours is { } group)
                            shown.Add(new ShownPaletteFxFrame(group, new PaletteFxFrame(reached.Program, show.Frame)));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.GoToStep goTo:
                        pending.Push(new PaletteFxReachedProgram(goTo.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.FallIntoStep fallInto:
                        pending.Push(new PaletteFxReachedProgram(fallInto.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.RepeatWhileCountingStep repeat:
                        pending.Push(new PaletteFxReachedProgram(repeat.Target, colours));
                        break;
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.CueStep:
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.HoldStep:
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.WaitStep:
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.SetCounterStep:
                    case AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.StopStep:
                        break;
                    default:
                        throw new InvalidOperationException($"Colour effect program {reached.Program} has an undefined step {step}.");
                }
            }
        }
    }
}
