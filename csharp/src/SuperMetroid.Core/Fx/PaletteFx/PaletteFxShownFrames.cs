using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The frames a set of colour effects' animations can show, each under the colour group its last event named (#1512,
/// #1598, #1600): each animation's frames, those of the phases it loops back to, and those of the animations it restarts
/// at above the Crateria lightning's height. The import pre-colours a shade for each; the ending names the shade family of each.
/// </summary>
internal static class PaletteFxShownFrames
{
    /// <summary>Every colour group and frame <paramref name="starts"/> can show, each once, ordered by group, program and frame.</summary>
    /// <exception cref="InvalidOperationException">A segment holds a beat of no defined kind.</exception>
    internal static ShownPaletteFxFrame[] From(IEnumerable<PaletteFxSegment> starts)
    {
        Ensure.NotNull(starts);
        var shown = new HashSet<ShownPaletteFxFrame>();
        foreach (PaletteFxSegment start in starts)
            Collect(start, shown);
        return [.. shown
            .OrderBy(entry => entry.Colours)
            .ThenBy(entry => entry.Frame.Animation)
            .ThenBy(entry => entry.Frame.Frame)];
    }

    /// <summary>The frames the initial programs of <paramref name="effects"/> can show, as <see cref="From"/> gives them.</summary>
    internal static ShownPaletteFxFrame[] ShownBy(IEnumerable<PaletteFxObject> effects) =>
        From(Ensure.NotNull(effects).Select(effect => RoomPaletteFxSelection.Get(effect).InitialAnimation));

    /// <summary>
    /// Adds every colour group and frame the animation starting at <paramref name="start"/> can show to
    /// <paramref name="shown"/>: each phase's frames under the colours named before them, the phases a loop returns to
    /// under the colours named when it does, and the animations its conditions restart it at.
    /// </summary>
    private static void Collect(PaletteFxSegment start, HashSet<ShownPaletteFxFrame> shown)
    {
        var visited = new HashSet<PaletteFxReachedAnimation>();
        var pending = new Stack<PaletteFxReachedAnimation>();
        pending.Push(new PaletteFxReachedAnimation(start, 0, null));
        while (pending.Count > 0)
        {
            PaletteFxReachedAnimation reached = pending.Pop();
            Animation<PaletteFxSegment> animation = PaletteFxAnimations.StartingWith(reached.Animation);
            PaletteFxColours? colours = reached.Colours;
            for (int phase = reached.Phase; phase < animation.Phases.Count; phase++)
            {
                if (!visited.Add(new PaletteFxReachedAnimation(reached.Animation, phase, colours)))
                    break;
                AnimationPhase<PaletteFxSegment> played = animation.Phases[phase];
                IReadOnlyList<AnimationBeat<int, PaletteFxCue>> beats = PaletteFxSegments.All.Beats(played.Segment);
                for (int beat = played.FirstBeat; beat < played.FirstBeat + played.BeatCount; beat++)
                {
                    switch (beats[beat])
                    {
                        case AnimationBeat<int, PaletteFxCue>.Event { Raised: PaletteFxCue.SetColours setColours }:
                            colours = setColours.Target;
                            break;
                        case AnimationBeat<int, PaletteFxCue>.Event { Raised: PaletteFxCue.Watch { Condition: ColourEffectCondition.RestartsWhileSamusIsHigh high } }:
                            pending.Push(new PaletteFxReachedAnimation(high.Restart, 0, colours));
                            break;
                        case AnimationBeat<int, PaletteFxCue>.Frame frame:
                            if (colours is { } group)
                                shown.Add(new ShownPaletteFxFrame(group, new PaletteFxFrame(played.Segment, frame.Shown)));
                            break;
                        case AnimationBeat<int, PaletteFxCue>.Event:
                            break;
                        default:
                            throw new InvalidOperationException($"Colour effect segment {played.Segment} has an undefined beat {beats[beat]}.");
                    }
                }
                if (phase == animation.Phases.Count - 1 && animation.End is AnimationEnd.Loop loop)
                    pending.Push(new PaletteFxReachedAnimation(reached.Animation, loop.Phase, colours));
            }
        }
    }
}
