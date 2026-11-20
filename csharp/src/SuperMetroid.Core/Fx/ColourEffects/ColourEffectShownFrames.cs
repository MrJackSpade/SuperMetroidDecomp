using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The frames a set of colour effects can show, each under its effect's colours (#1512, #1598, #1600, #1982, #2366):
/// every frame of every run an effect's animation plays, its loop returning only to one of them. The import pre-colours a
/// shade for each; the ending names the shade family of each.
/// </summary>
internal static class ColourEffectShownFrames
{
    /// <summary>
    /// Every colour group and frame <paramref name="effects"/> can show, each once, ordered by group and effect in their
    /// written-out orders (<see cref="ColourEffectListedOrders"/>, #2567), then by frame. Samus
    /// in heat shows none of her own: her suit's pulse is the effect that shows her shimmer's frames.
    /// </summary>
    /// <exception cref="InvalidOperationException">An effect plays a beat of no defined kind.</exception>
    internal static ShownColourEffectFrame[] ShownBy(IEnumerable<ColourEffectId> effects)
    {
        Ensure.NotNull(effects);
        var shown = new HashSet<ShownColourEffectFrame>();
        foreach (ColourEffectId effect in effects)
        {
            ColourEffectColours colours = ColourEffectAnimations.Definition(effect).Colours;
            foreach (AnimationBeat<int, ColourEffectCue> beat in Steps(effect))
            {
                switch (beat)
                {
                    case AnimationBeat<int, ColourEffectCue>.Frame frame:
                        shown.Add(new ShownColourEffectFrame(colours, new ColourEffectFrame(effect, frame.Shown)));
                        break;
                    case AnimationBeat<int, ColourEffectCue>.Event:
                        break;
                    default:
                        throw new InvalidOperationException($"Colour effect {effect} has an undefined step {beat}.");
                }
            }
        }
        return [.. shown
            .OrderBy(entry => ColourEffectListedOrders.Groups.PlaceOf(entry.Colours))
            .ThenBy(entry => ColourEffectListedOrders.Effects.PlaceOf(entry.Frame.Effect))
            .ThenBy(entry => entry.Frame.Frame)];
    }

    /// <summary>
    /// The steps <paramref name="effect"/> plays, in its own frames: the beats of each run its animation plays, each run
    /// once, in the order the animation first plays them, which numbers its frames from zero in the order it shows them.
    /// </summary>
    internal static IEnumerable<AnimationBeat<int, ColourEffectCue>> Steps(ColourEffectId effect) =>
        ColourEffectAnimations.Definition(effect).Frames.OwnSegments.Distinct()
            .SelectMany(segment => ColourEffectAnimations.Segments.Beats(segment));
}
