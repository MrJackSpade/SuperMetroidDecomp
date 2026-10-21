namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The quadrants of the left and right edges of Mother Brain's beam target.</summary>
/// <param name="LeftEdge">The quadrant of the target's left edge.</param>
/// <param name="RightEdge">The quadrant of the target's right edge.</param>
internal readonly record struct MotherBrainBeamQuadrants(int LeftEdge, int RightEdge);
