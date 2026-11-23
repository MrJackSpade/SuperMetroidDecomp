namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A vertical displacement after clipping to a slope, and whether it collided.</summary>
/// <param name="Displacement">The vertical displacement the slope leaves of the move.</param>
/// <param name="Collided">Whether the slope stopped the move.</param>
public readonly partial record struct SlopeClip(SignedSixteenSixteen Displacement, bool Collided);
