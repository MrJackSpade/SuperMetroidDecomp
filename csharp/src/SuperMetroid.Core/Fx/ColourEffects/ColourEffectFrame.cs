namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One frame of a colour effect's animation (#1598, #1739): the animation that shows it and its place among the frames that
/// animation shows. The import pre-colours the sheets each frame shows from its installed colours.
/// </summary>
/// <param name="Animation">The animation that shows the frame.</param>
/// <param name="Frame">The frame's place among the frames <paramref name="Animation"/> shows, from zero.</param>
public readonly record struct ColourEffectFrame(ColourEffectAnimation Animation, int Frame);
