using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The frames a set of colour effects' animations can show, each under its effect's colours (#1512, #1598, #1600, #1982):
/// every frame of every phase an animation plays, the phases its loop returns to among them. The import pre-colours a
/// shade for each; the ending names the shade family of each.
/// </summary>
internal static class ColourEffectShownFrames
{
    /// <summary>Every colour group and frame <paramref name="animations"/> can show, each once, ordered by group, effect and frame.</summary>
    /// <exception cref="InvalidOperationException">An animation holds a step of no defined kind.</exception>
    internal static ShownColourEffectFrame[] From(IEnumerable<Animation<ColourEffectId>> animations)
    {
        Ensure.NotNull(animations);
        var shown = new HashSet<ShownColourEffectFrame>();
        foreach (Animation<ColourEffectId> animation in animations)
            Collect(animation, shown);
        return [.. shown
            .OrderBy(entry => entry.Colours)
            .ThenBy(entry => entry.Frame.Effect)
            .ThenBy(entry => entry.Frame.Frame)];
    }

    /// <summary>The frames the animations of <paramref name="effects"/> can show, as <see cref="From"/> gives them.</summary>
    internal static ShownColourEffectFrame[] ShownBy(IEnumerable<ColourEffectId> effects) =>
        From(Ensure.NotNull(effects).Select(effect => ColourEffectAnimations.Definition(effect).Animation));

    /// <summary>
    /// Adds every colour group and frame <paramref name="animation"/> can show to <paramref name="shown"/>: each phase's
    /// frames, under the colours of the effect whose steps the phase plays. Every phase is reached: an animation plays its
    /// phases in order and a loop only returns to one of them.
    /// </summary>
    private static void Collect(Animation<ColourEffectId> animation, HashSet<ShownColourEffectFrame> shown)
    {
        foreach (AnimationPhase<ColourEffectId> played in animation.Phases)
        {
            ColourEffectColours colours = ColourEffectAnimations.Definition(played.Segment).Colours;
            IReadOnlyList<AnimationBeat<int, ColourEffectCue>> beats = ColourEffectAnimations.Steps.Beats(played.Segment);
            for (int beat = played.FirstBeat; beat < played.FirstBeat + played.BeatCount; beat++)
            {
                switch (beats[beat])
                {
                    case AnimationBeat<int, ColourEffectCue>.Frame frame:
                        shown.Add(new ShownColourEffectFrame(colours, new ColourEffectFrame(played.Segment, frame.Shown)));
                        break;
                    case AnimationBeat<int, ColourEffectCue>.Event:
                        break;
                    default:
                        throw new InvalidOperationException($"Colour effect {played.Segment} has an undefined step {beats[beat]}.");
                }
            }
        }
    }
}
