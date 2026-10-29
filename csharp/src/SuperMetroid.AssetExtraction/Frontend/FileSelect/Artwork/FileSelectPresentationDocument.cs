using System.Text.Json.Serialization;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>Authored JSON layout and artwork for file-select menus: the layout the game binds and the sprite compositions the file-select sheet is composed from (#1463).</summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries. A loaded presentation parses its own document and compiles artwork from the supplied JSON stream.</remarks>
public sealed record FileSelectPresentationDocument : FileSelectLayoutDocument
{
    /// <summary>Exactly the named border compositions, four cursor frames, and eight helmet frames, using sprite-part offsets relative to their drawing anchors.</summary>
    [JsonPropertyOrder(8)]
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
}
