namespace SuperMetroid.Core.Assets;

/// <summary>Defines the four directional map-arrow visuals.</summary>
public sealed record MapArrowDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact Left, Right, Up, and Down arrow entries.</summary>
    public required Dictionary<string, MapArrowEntry> Arrows { get; init; }
}
