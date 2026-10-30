using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Human-readable visual tile words; DMA timing and destinations are not editable.</summary>
internal sealed record CeresEscapeOverlayTilemapDocument
{
    /// <summary>Schema revision checked before compiling warning-page edits.</summary>
    public int Version { get; init; }
    /// <summary>Five named arrays of complete BG tile words, one per fixed Ceres overlay.</summary>
    public required Dictionary<string, BackgroundTile[]> Pages { get; init; }
}
