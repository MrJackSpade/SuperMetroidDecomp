using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>Authored options-menu JSON: background tilemaps, controller glyphs, palette-only highlight regions, and pixel-space sprite layouts.</summary>
/// <remarks>Init-only properties retain caller-owned mutable arrays and dictionaries. The stream loader parses its own document before compiling the runtime presentation.</remarks>
public sealed record GameOptionsPresentationDocument
{
    /// <summary>Schema version that must equal <see cref="GameOptionsPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six named pages, each containing 1024 row-major cells for a 32-by-32 tilemap.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
    /// <summary>Seven named assignable-button glyph patches, each containing six row-major cells forming a 3-by-2 tile rectangle.</summary>
    public required Dictionary<string, MapPresentationCell[]> ControllerLabels { get; init; }
    /// <summary>Seven controller-action glyph origins in action order, in tile units; each 3-by-2 label must fit the page.</summary>
    public required Position16<TilemapCells>[] ControllerLabelAnchors { get; init; }
    /// <summary>Four nonempty, mutually disjoint primary-page cell regions whose palettes reflect the selected language.</summary>
    public required GameOptionsLanguageRegionDocument[] LanguageRegions { get; init; }
    /// <summary>Exactly the icon-cancel and moonwalk choice regions; these select visual palettes without changing option behavior.</summary>
    public required Dictionary<string, GameOptionsToggleVisualDocument> SpecialToggles { get; init; }
    /// <summary>Exactly three menu-heading compositions and four missile-cursor frames, using sprite-part offsets from drawing anchors.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Sprites { get; init; }
    /// <summary>Heading origins for primary, controller, and special menus in screen pixels, X=0-255 and Y=0-255; drawing subtracts vertical scroll from Y.</summary>
    public required Dictionary<string, Position16<SpritePixels>> HeadingAnchors { get; init; }
    /// <summary>Cursor origins in selection-row order for each menu: five primary, nine controller, and three special points, in screen pixels X=0-255 and Y=0-255.</summary>
    public required Dictionary<string, Position16<SpritePixels>[]> CursorAnchors { get; init; }
    /// <summary>Cursor origin for phases without a selection table, in screen pixels X=0-511 and Y=0-255, allowing offscreen placement.</summary>
    public required Position16<SpritePixels> HiddenCursor { get; init; }
    /// <summary>BG palette index 0-7 for chosen language and toggle regions; must differ from the unselected index.</summary>
    public required int SelectedPalette { get; init; }
    /// <summary>BG palette index 0-7 for language and toggle regions that are not chosen.</summary>
    public required int UnselectedPalette { get; init; }
    /// <summary>OBJ palette index 0-7 shared by missile-cursor and heading compositions.</summary>
    public required int CursorPalette { get; init; }
    /// <summary>Duration 1-65535 in menu updates for each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
