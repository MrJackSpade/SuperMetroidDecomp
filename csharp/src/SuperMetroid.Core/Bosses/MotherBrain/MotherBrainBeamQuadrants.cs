namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The quadrants of the left and right edges of Mother Brain's beam target.</summary>
/// <param name="LeftEdge">The quadrant of the target's left edge.</param>
/// <param name="RightEdge">The quadrant of the target's right edge.</param>
internal readonly record struct MotherBrainBeamQuadrants(int LeftEdge, int RightEdge)
{
    /// <summary>
    /// The way the beam opens for edge-quadrant pair <paramref name="pair"/> (left-edge quadrant times four plus
    /// right-edge quadrant), as the cartridge's rasterizer table at $AD:DE5F selects it.
    /// </summary>
    internal static MotherBrainBeamDirection DirectionForPair(int pair)
    {
        if ((uint)pair >= 16) throw new ArgumentOutOfRangeException(nameof(pair));
        return new MotherBrainBeamQuadrants(pair / 4, pair % 4) switch
        {
            (0, 0) or (3, 0) or (3, 3) => MotherBrainBeamDirection.Down,
            (0, 1) => MotherBrainBeamDirection.Right,
            (1, 1) or (1, 2) or (2, 2) => MotherBrainBeamDirection.Up,
            (2, 3) => MotherBrainBeamDirection.Retain,
            _ => MotherBrainBeamDirection.Unsupported,
        };
    }
}
