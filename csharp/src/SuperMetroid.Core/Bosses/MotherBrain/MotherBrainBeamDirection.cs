namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The way Mother Brain's rainbow beam cone opens, chosen from the quadrants of its two edges.</summary>
public enum MotherBrainBeamDirection
{
    /// <summary>Both edges below the horizontal axis (the cartridge's downward rasterizer).</summary>
    Down,
    /// <summary>Both edges above the horizontal axis (the cartridge's upward rasterizer).</summary>
    Up,
    /// <summary>The edges straddle the rightward axis (the cartridge's rightward rasterizer).</summary>
    Right,
    /// <summary>The edges straddle the leftward axis: the cartridge returns at once, so the preceding cone is kept rather than rebuilt.</summary>
    Retain,
    /// <summary>A quadrant pair the cartridge has no rasterizer for (its table holds zero); the beam rejects it.</summary>
    Unsupported
}
