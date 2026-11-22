namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One frame of a colour effect's animation (#1598, #1739, #1982): the effect that shows it and its place among the frames
/// that effect shows. The import pre-colours the sheets each frame shows from its installed colours.
/// </summary>
/// <param name="Effect">The effect that shows the frame.</param>
/// <param name="Frame">The frame's place among the frames <paramref name="Effect"/> shows, from zero.</param>
public readonly partial record struct ColourEffectFrame(ColourEffectId Effect, int Frame);
