using System.Text.Json.Serialization;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>Authored options-menu JSON: the layout the game binds and the heading and cursor compositions the options sheet is composed from (#1463).</summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries. The stream loader parses its own document before compiling the runtime presentation.</remarks>
public sealed record GameOptionsPresentationDocument : GameOptionsLayoutDocument
{
    /// <summary>Exactly three menu-heading compositions and four missile-cursor frames, using sprite-part offsets from drawing anchors.</summary>
    [JsonPropertyOrder(7)]
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
}
