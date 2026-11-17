namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A vertical displacement after clipping to a slope, and whether it collided.</summary>
/// <param name="Displacement">The vertical displacement the slope leaves of the move.</param>
/// <param name="Collided">Whether the slope stopped the move.</param>
[IndependentFlag("whether the slope stopped the move, a fact of the block and the displacement")]
public readonly record struct SlopeClip(SignedSixteenSixteen Displacement, bool Collided);
