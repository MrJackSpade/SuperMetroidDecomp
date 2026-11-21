namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable appearance-only schema for the single grapple endpoint and four timed rope styles; the segment array remains caller-mutable until compilation.</summary>
public sealed record GrappleSpriteDocument
{
    /// <summary>Schema revision; loading requires <see cref="GrappleSpriteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nonnull endpoint appearance shared by connected and unconnected drawing; its position remains owned by the live grapple state.</summary>
    public required GrappleSpriteStyle Endpoint { get; init; }
    /// <summary>Each of the rope's four pictures' appearance, keyed by the picture's name; no duration or instruction fields are editable here.</summary>
    public required Dictionary<string, GrappleSpriteStyle> Segments { get; init; }
}
