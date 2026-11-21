using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's images as installed (#2565): her body and brain's colours and her rear legs' colours together, so
/// neither is matched to the other by its place in a second list.
/// </summary>
public sealed record MotherBrainBodyColoursDocument
{
    /// <summary>Her body and brain's colours.</summary>
    public required PaletteRgb5[] Body { get; init; }

    /// <summary>Her rear legs' colours.</summary>
    public required PaletteRgb5[] BackLegs { get; init; }
}
