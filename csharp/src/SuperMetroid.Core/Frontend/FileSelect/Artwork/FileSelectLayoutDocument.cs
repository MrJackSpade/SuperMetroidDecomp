using System.Text.Json.Serialization;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// The file-select JSON's pages, fields and layout the game binds, with tile-space background fields and pixel-space
/// sprite anchors. The document the import pipeline reads also holds the sprite compositions the file-select sheet is
/// composed from (#1463); each member keeps its place in the document.
/// </summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries. A loaded presentation parses its own document and compiles artwork from the supplied JSON stream.</remarks>
public record FileSelectLayoutDocument
{
    /// <summary>Schema version; must equal <see cref="FileSelectPresentationDefinitions.Version"/>.</summary>
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
    /// <summary>Three dynamic save-field layouts in slot order for the main file-select page.</summary>
    [JsonPropertyOrder(6)]
    public required FileSelectSlotFieldDocument[] MainSlots { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for copy and clear pages.</summary>
    [JsonPropertyOrder(7)]
    public required FileSelectSlotFieldDocument[] DataSlots { get; init; }
    /// <summary>Main, copy, and clear border origins in screen pixels, with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(9)]
    public required Dictionary<string, Position16<SpritePixels>> BorderAnchors { get; init; }
    /// <summary>Six missile-cursor origins in main-menu selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(10)]
    public required Position16<SpritePixels>[] MainCursorAnchors { get; init; }
    /// <summary>Four missile-cursor origins in data-management selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(11)]
    public required Position16<SpritePixels>[] DataCursorAnchors { get; init; }
    /// <summary>Two missile-cursor origins in confirmation-option order, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(12)]
    public required Position16<SpritePixels>[] ConfirmationCursorAnchors { get; init; }
    /// <summary>Three helmet origins in save-slot order, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(13)]
    public required Position16<SpritePixels>[] HelmetAnchors { get; init; }
    /// <summary>Exactly the four named copy/clear slot-letter anchors, in tile coordinates inside the 32-by-32 page.</summary>
    [JsonPropertyOrder(14)]
    public required Dictionary<string, Position16<TilemapCells>> DynamicAnchors { get; init; }
    /// <summary>OBJ palette selector 0-7 applied to border, cursor, and helmet compositions.</summary>
    [JsonPropertyOrder(15)]
    public required int ObjectPalette { get; init; }
    /// <summary>Menu-update duration 1-65535 for each of the four looping missile-cursor frames.</summary>
    [JsonPropertyOrder(16)]
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu-update duration 1-65535 for each selected-slot helmet frame; the engine stops the animation on its last frame.</summary>
    [JsonPropertyOrder(17)]
    public required int HelmetFrameDuration { get; init; }
}
