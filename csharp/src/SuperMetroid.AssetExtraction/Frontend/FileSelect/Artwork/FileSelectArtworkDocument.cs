using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The file-select artwork JSON, import data (#1638): the pages, patches, glyphs and sprite compositions the import composes
/// the file-select pictures from. Their cells and parts are the original's packed tile placements, so the installation never
/// holds them; it holds the pictures, and the layout the game binds (<see cref="FileSelectLayoutDocument"/>).
/// </summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries.</remarks>
public sealed record FileSelectArtworkDocument
{
    /// <summary>Schema version; must equal <see cref="FileSelectPresentationFormat.ArtworkVersion"/>.</summary>
    [JsonPropertyOrder(1)]
    public required int Version { get; init; }
    /// <summary>Exactly the named menu pages, each containing 1024 row-major cells for a 32-by-32 tilemap.</summary>
    [JsonPropertyOrder(2)]
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    /// <summary>Exactly the energy, empty-slot, and time-colon patches, placed relative to dynamic slot-field anchors.</summary>
    [JsonPropertyOrder(3)]
    public required Dictionary<string, FileSelectPatchDocument> Patches { get; init; }
    /// <summary>Ten tile cells in digit-value order 0-9, including each glyph's palette and tile attributes.</summary>
    [JsonPropertyOrder(4)]
    public required MapPresentationCell[] Digits { get; init; }
    /// <summary>Three tile cells in save-slot order A-C, used when copy or clear text identifies a slot.</summary>
    [JsonPropertyOrder(5)]
    public required MapPresentationCell[] SlotLetters { get; init; }
    /// <summary>Exactly the named border compositions, four cursor frames, and eight helmet frames, using sprite-part offsets relative to their drawing anchors.</summary>
    [JsonPropertyOrder(8)]
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>OBJ palette selector 0-7 applied to border, cursor, and helmet compositions.</summary>
    [JsonPropertyOrder(15)]
    public required int ObjectPalette { get; init; }
}
