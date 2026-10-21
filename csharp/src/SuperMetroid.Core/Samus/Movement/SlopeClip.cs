namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A vertical displacement after clipping to a slope, and whether it collided.</summary>
/// <param name="Displacement">The displacement.</param>
/// <param name="Collided">The collided.</param>
[IndependentFlag("whether the slope stopped the move, a fact of the block and the displacement")]
public readonly record struct SlopeClip(int Displacement, bool Collided);
