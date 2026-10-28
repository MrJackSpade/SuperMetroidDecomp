using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Immutable file-select pages, dynamic field artwork and layout the game binds. The actors are drawn from the installed
/// file-select sheet, whose compositions belong to the import pipeline that reads the installed presentation (#1463).
/// SRAM validation, menu navigation, copy/clear operations and fades remain compiled.
/// </summary>
public abstract class FileSelectPresentation
{
    /// <summary>Named, compiled 32-by-32 BG2 tilemaps for the menu pages.</summary>
    private readonly Dictionary<FileSelectPage, BackgroundTile[]> pages;
    /// <summary>The common BG2 background tilemap.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>Each slot-field patch.</summary>
    internal readonly Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches;
    /// <summary>Edited digit glyphs, or null when every digit is the native consecutive glyph.</summary>
    internal readonly BackgroundTile[]? digits;
    /// <summary>Edited slot-letter glyphs, or null when every letter is the native consecutive glyph.</summary>
    internal readonly BackgroundTile[]? slotLetters;
    /// <summary>Each bordered heading's anchor.</summary>
    private readonly Dictionary<FileSelectBorder, Position16<SpritePixels>> borderAnchors;
    /// <summary>Each copy or clear page's slot-letter anchor.</summary>
    private readonly Dictionary<FileSelectAnchor, Position16<TilemapCells>> dynamicAnchors;
    /// <summary>Validated anchors and frame durations from the selected JSON presentation.</summary>
    private readonly FileSelectLayoutDocument document;

    /// <summary>Stores compiled artwork and collapses unedited glyph sequences to stock tile bases.</summary>
    /// <param name="document">The validated layout.</param>
    /// <param name="pages">Each foreground page's compiled tiles.</param>
    /// <param name="background">The common BG2 background's compiled tiles.</param>
    /// <param name="patches">Each slot-field patch.</param>
    /// <param name="digits">The ten digit glyphs.</param>
    /// <param name="slotLetters">The three slot-letter glyphs.</param>
    /// <param name="contentIdentity">The identity of the selected presentation's bytes.</param>
    internal FileSelectPresentation(
        FileSelectLayoutDocument document,
        Dictionary<FileSelectPage, BackgroundTile[]> pages,
        BackgroundTile[] background,
        Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches,
        BackgroundTile[] digits,
        BackgroundTile[] slotLetters,
        string contentIdentity)
    {
        this.pages = pages;
        this.background = background;
        this.patches = patches;
        this.digits = PreserveEditedGlyphs(digits, FileSelectLayout.DigitTileBase);
        this.slotLetters = PreserveEditedGlyphs(slotLetters, FileSelectLayout.SamusLetterTileBase);
        borderAnchors = FileSelectPresentationDefinitions.Borders.ToDictionary(border => border,
            border => document.BorderAnchors[FileSelectParts.BorderName(border)]);
        dynamicAnchors = FileSelectPresentationDefinitions.Anchors.ToDictionary(anchor => anchor,
            anchor => document.DynamicAnchors[FileSelectParts.AnchorName(anchor)]);
        this.document = document;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Returns null when every glyph matches its stock consecutive tile.</summary>
    private static BackgroundTile[]? PreserveEditedGlyphs(BackgroundTile[] values, BackgroundTile first)
    {
        for (int index = 0; index < values.Length; index++)
            if (values[index] != first with { Tile = first.Tile + index })
                return values;
        return null;
    }
    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public int CursorFrameDuration => document.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public int HelmetFrameDuration => document.HelmetFrameDuration;

    /// <summary>Copies the background page into the menu's BG2 VRAM tilemap.</summary>
    internal void LoadBackground(MenuTilemaps tilemaps) =>
        background.CopyTo(tilemaps.Bg2);

    /// <summary>Copies one named full page into a 1024-cell destination tilemap.</summary>
    internal void CopyPage(FileSelectPage page, Span<BackgroundTile> destination)
    {
        Ensure.LengthEqual(destination, FileSelectPresentationDefinitions.CellCount);
        pages[page].CopyTo(destination);
    }

    /// <summary>Selects the main or data-management field anchors for one save slot.</summary>
    internal FileSelectSlotFieldDocument Slot(bool dataManagement, int slot)
    {
        FileSelectSlotFieldDocument[] layouts = dataManagement
            ? document.DataSlots
            : document.MainSlots;
        return (uint)slot < layouts.Length
            ? layouts[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot));
    }

    /// <summary>Places an ENERGY, NO DATA, or time-colon patch relative to its tile anchor.</summary>
    internal void ApplyPatch(Span<BackgroundTile> tilemap, FileSelectPatch name, Position16<TilemapCells> anchor)
    {
        FileSelectCompiledPatch patch = patches[name];
        for (int index = 0; index < patch.Count; index++)
        {
            FileSelectCompiledPatchCell cell = patch.Cell(index);
            tilemap[FileSelectPresentationDefinitions.PageSize.RowMajorIndex(anchor + cell.Offset)] = cell.Tile;
        }
    }

    /// <summary>Writes one decimal digit using edited glyph art when supplied.</summary>
    internal void WriteDigit(Span<BackgroundTile> tilemap, Position16<TilemapCells> anchor, int offset, int digit)
    {
        if ((uint)digit >= FileSelectPresentationDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException(nameof(digit));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X + offset] =
            digits is null ? FileSelectLayout.Digit(digit) : digits[digit];
    }

    /// <summary>Writes the selected slot's letter at its dynamic tile anchor.</summary>
    internal void WriteSlotLetter(Span<BackgroundTile> tilemap, Position16<TilemapCells> anchor, int slot)
    {
        if ((uint)slot >= FileSelectPresentationDefinitions.SlotLetterCount)
            throw new ArgumentOutOfRangeException(nameof(slot));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X] =
            slotLetters is null ? FileSelectLayout.SlotLetter(slot) : slotLetters[slot];
    }

    /// <summary>Resolves a named dynamic field location in the menu tilemap.</summary>
    internal Position16<TilemapCells> DynamicAnchor(FileSelectAnchor anchor) => dynamicAnchors[anchor];

    /// <summary>Selects the cursor anchor from main, confirmation, or data-management layouts.</summary>
    internal Position16<SpritePixels> CursorPosition(FileSelectCursorPage page, int selected) =>
        Point(page switch
        {
            FileSelectCursorPage.Main => document.MainCursorAnchors,
            FileSelectCursorPage.DataManagement => document.DataCursorAnchors,
            FileSelectCursorPage.Confirmation => document.ConfirmationCursorAnchors,
            _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectCursorPage)} {page}."),
        }, selected, "cursor");

    /// <summary>The OBJ palette the borders, cursor and helmets draw in.</summary>
    internal int ObjectPalette => document.ObjectPalette;

    /// <summary>Draws the page's border frame of the file-select sheet at its authored OAM anchor.</summary>
    internal void DrawBorder(ScreenSprites screenSprites, SpriteSheet sheet, FileSelectBorder border) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.BorderSpriteName(border)), borderAnchors[border]);

    /// <summary>Draws one missile-cursor frame of the file-select sheet.</summary>
    internal static void DrawCursor(ScreenSprites screenSprites, SpriteSheet sheet, int frame, Position16<SpritePixels> anchor) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.CursorSpriteName(frame)), anchor);

    /// <summary>Draws a selected-slot helmet frame of the file-select sheet at that slot's OAM anchor.</summary>
    internal void DrawHelmet(ScreenSprites screenSprites, SpriteSheet sheet, int frame, int slot) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.HelmetSpriteName(frame)),
            Point(document.HelmetAnchors, slot, "helmet"));

    /// <summary>Reads an indexed OAM anchor, rejecting selections outside its layout.</summary>
    private static Position16<SpritePixels> Point(Position16<SpritePixels>[] points, int index, string owner) =>
        (uint)index < points.Length ? points[index] :
            throw new ArgumentOutOfRangeException(owner);
}
