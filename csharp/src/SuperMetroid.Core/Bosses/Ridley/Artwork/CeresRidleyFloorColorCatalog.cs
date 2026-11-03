namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyFloorColorCatalog
{
    /// <summary>Immutable decoded shades indexed by getaway zoom step.</summary>
    internal readonly CeresRidleyFloorPaint steps;

    /// <summary>Keeps the recognized zoom-shade selector.</summary>
    /// <param name="steps">The zoom shades the import recognized.</param>
    internal CeresRidleyFloorColorCatalog(CeresRidleyFloorPaint steps) => this.steps = steps;
}
