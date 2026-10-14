namespace SuperMetroid.Core.Assets;

/// <summary>Human-readable visual tile words; DMA timing and destinations are not editable.</summary>
internal sealed record CeresEscapeOverlayTilemapDocument
{
    /// <summary>Schema revision checked before compiling warning-page edits.</summary>
    public int Version { get; init; }
    /// <summary>Five named arrays of complete BG tile words, one per fixed Ceres overlay.</summary>
    public required Dictionary<string, ushort[]> Pages { get; init; }
}
