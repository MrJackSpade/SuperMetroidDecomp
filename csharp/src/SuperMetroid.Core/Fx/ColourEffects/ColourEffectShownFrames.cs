using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The frames a set of colour effects' animations can show, each under the colour group its last event named (#1512,
/// #1598, #1600): each animation's frames, those of the phases it loops back to, and those of the cycle it restarts above
/// the Crateria lightning's height. The import pre-colours a shade for each; the ending names the shade family of each.
/// </summary>
internal static class ColourEffectShownFrames
{
    /// <summary>Every colour group and frame <paramref name="starts"/> can show, each once, ordered by group, animation and frame.</summary>
    /// <exception cref="InvalidOperationException">An animation holds a step of no defined kind.</exception>
    internal static ShownColourEffectFrame[] From(IEnumerable<ColourEffectAnimation> starts)
    {
        Ensure.NotNull(starts);
        var shown = new HashSet<ShownColourEffectFrame>();
        foreach (ColourEffectAnimation start in starts)
            Collect(start, shown);
        return [.. shown
            .OrderBy(entry => entry.Colours)
            .ThenBy(entry => entry.Frame.Animation)
            .ThenBy(entry => entry.Frame.Frame)];
    }

    /// <summary>The frames the initial animations of <paramref name="effects"/> can show, as <see cref="From"/> gives them.</summary>
    internal static ShownColourEffectFrame[] ShownBy(IEnumerable<ColourEffectId> effects) =>
        From(Ensure.NotNull(effects).Select(effect => ColourEffectSelection.Get(effect).InitialAnimation));

    /// <summary>
    /// Adds every colour group and frame the animation <paramref name="start"/> can show to <paramref name="shown"/>: each
    /// phase's frames under the colours named before them, the phases a loop returns to under the colours named when it
    /// does, and the cycle its condition restarts it at.
    /// </summary>
    private static void Collect(ColourEffectAnimation start, HashSet<ShownColourEffectFrame> shown)
    {
        var visited = new HashSet<ColourEffectReachedPhase>();
        var pending = new Stack<ColourEffectReachedPhase>();
        pending.Push(new ColourEffectReachedPhase(start, 0, null));
        while (pending.Count > 0)
        {
            ColourEffectReachedPhase reached = pending.Pop();
            Animation<ColourEffectAnimation> animation = ColourEffectAnimations.Of(reached.Animation);
            ColourEffectColours? colours = reached.Colours;
            for (int phase = reached.Phase; phase < animation.Phases.Count; phase++)
            {
                if (!visited.Add(new ColourEffectReachedPhase(reached.Animation, phase, colours)))
                    break;
                AnimationPhase<ColourEffectAnimation> played = animation.Phases[phase];
                IReadOnlyList<AnimationBeat<int, ColourEffectCue>> beats = ColourEffectAnimations.Steps.Beats(played.Segment);
                for (int beat = played.FirstBeat; beat < played.FirstBeat + played.BeatCount; beat++)
                {
                    switch (beats[beat])
                    {
                        case AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.SetColours setColours }:
                            colours = setColours.Target;
                            break;
                        case AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.Watch { Condition: ColourEffectCondition.RestartsWhileSamusIsHigh } }:
                            pending.Push(new ColourEffectReachedPhase(reached.Animation, animation.CyclePhase, colours));
                            break;
                        case AnimationBeat<int, ColourEffectCue>.Frame frame:
                            if (colours is { } group)
                                shown.Add(new ShownColourEffectFrame(group, new ColourEffectFrame(played.Segment, frame.Shown)));
                            break;
                        case AnimationBeat<int, ColourEffectCue>.Event:
                            break;
                        default:
                            throw new InvalidOperationException($"Colour effect animation {played.Segment} has an undefined step {beats[beat]}.");
                    }
                }
                if (phase == animation.Phases.Count - 1 && animation.End is AnimationEnd.Loop loop)
                    pending.Push(new ColourEffectReachedPhase(reached.Animation, loop.Phase, colours));
            }
        }
    }
}
