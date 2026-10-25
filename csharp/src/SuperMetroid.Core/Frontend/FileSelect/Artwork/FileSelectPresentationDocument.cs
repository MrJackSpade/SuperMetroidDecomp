using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Authored JSON layout and artwork for file-select menus, with tile-space background fields and pixel-space sprite anchors.</summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries. A loaded presentation parses its own document and compiles artwork from the supplied JSON stream.</remarks>
public sealed record FileSelectPresentationDocument
{
    /// <summary>Schema version; must equal <see cref="FileSelectPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly the named menu pages, each containing 1024 row-major cells for a 32-by-32 tilemap.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    /// <summary>Exactly the energy, empty-slot, and time-colon patches, placed relative to dynamic slot-field anchors.</summary>
    public required Dictionary<string, FileSelectPatchDocument> Patches { get; init; }
    /// <summary>Ten tile cells in digit-value order 0-9, including each glyph's palette and tile attributes.</summary>
    public required MapPresentationCell[] Digits { get; init; }
    /// <summary>Three tile cells in save-slot order A-C, used when copy or clear text identifies a slot.</summary>
    public required MapPresentationCell[] SlotLetters { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for the main file-select page.</summary>
    public required FileSelectSlotFieldDocument[] MainSlots { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for copy and clear pages.</summary>
    public required FileSelectSlotFieldDocument[] DataSlots { get; init; }
    /// <summary>Exactly the named border compositions, four cursor frames, and eight helmet frames, using sprite-part offsets relative to their drawing anchors.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>Main, copy, and clear border origins in screen pixels, with X=0-511 and Y=0-255.</summary>
    public required Dictionary<string, Position16<SpritePixels>> BorderAnchors { get; init; }
    /// <summary>Six missile-cursor origins in main-menu selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required Position16<SpritePixels>[] MainCursorAnchors { get; init; }
    /// <summary>Four missile-cursor origins in data-management selection order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required Position16<SpritePixels>[] DataCursorAnchors { get; init; }
    /// <summary>Two missile-cursor origins in confirmation-option order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required Position16<SpritePixels>[] ConfirmationCursorAnchors { get; init; }
    /// <summary>Three helmet origins in save-slot order, in screen pixels with X=0-511 and Y=0-255.</summary>
    public required Position16<SpritePixels>[] HelmetAnchors { get; init; }
    /// <summary>Exactly the four named copy/clear slot-letter anchors, in tile coordinates inside the 32-by-32 page.</summary>
    public required Dictionary<string, Position16<TilemapCells>> DynamicAnchors { get; init; }
    /// <summary>OBJ palette selector 0-7 applied to border, cursor, and helmet compositions.</summary>
    public required int ObjectPalette { get; init; }
    /// <summary>Menu-update duration 1-65535 for each of the four looping missile-cursor frames.</summary>
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu-update duration 1-65535 for each selected-slot helmet frame; the engine stops the animation on its last frame.</summary>
    public required int HelmetFrameDuration { get; init; }
}
