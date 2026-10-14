namespace SuperMetroid.Core.Assets;

/// <summary>Defines every named map-screen sprite composition.</summary>
public sealed record MapSpriteDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact 26-frame dictionary of ordered OBJ parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
