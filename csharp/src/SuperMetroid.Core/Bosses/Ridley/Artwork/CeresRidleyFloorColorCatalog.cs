using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyFloorColorCatalog
{
    /// <summary>Immutable decoded shades indexed by getaway zoom step.</summary>
    internal readonly CeresRidleyFloorPaint steps;

    /// <summary>Copies validated RGB5 steps into the fixed zoom-shade selector.</summary>
    internal CeresRidleyFloorColorCatalog(RgbColor[][] steps) => this.steps = new(steps);

    /// <summary>Reproduces $A6:B0EF's fifteen-color zoom shade write to CGRAM 81..95, preserving BG palette 5 color zero and leaving zoom/rotation mechanics unchanged.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="zoomStep">The getaway zoom step: the whole part of the current zoom, 0..8; not a frame timer or byte offset into the stored steps.</param>
    /// <exception cref="ArgumentOutOfRangeException">The zoom step is outside 0..8.</exception>
    public void Apply(GameplayColours colours, int zoomStep) => CeresRidleyFloorShades.Apply(steps, colours, zoomStep);
}
