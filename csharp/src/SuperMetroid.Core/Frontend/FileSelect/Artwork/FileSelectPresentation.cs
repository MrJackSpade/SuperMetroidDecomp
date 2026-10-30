using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Immutable file-select layout the game binds: each slot field's anchors, the copy and clear pages' slot-letter anchors,
/// the cursor and helmet anchors and the frame durations. The pages, field patches and glyphs are pictures the import
/// composes once (#1467); the actors are drawn from the installed file-select sheet, whose compositions belong to the
/// import pipeline that reads the installed presentation (#1463). SRAM validation, menu navigation, copy/clear
/// operations and fades remain compiled.
/// </summary>
public abstract class FileSelectPresentation
{
    /// <summary>Each bordered heading's anchor.</summary>
    private readonly Dictionary<FileSelectBorder, Position16<SpritePixels>> borderAnchors;
    /// <summary>Each copy or clear page's slot-letter anchor.</summary>
    private readonly Dictionary<FileSelectAnchor, Position16<TilemapCells>> dynamicAnchors;
    /// <summary>Validated anchors and frame durations from the selected JSON presentation.</summary>
    private readonly FileSelectLayoutDocument document;

    /// <summary>Keeps the layout's anchors and durations.</summary>
    /// <param name="document">The validated layout.</param>
    /// <param name="contentIdentity">The identity of the selected presentation's bytes.</param>
    internal FileSelectPresentation(FileSelectLayoutDocument document, string contentIdentity)
    {
        borderAnchors = FileSelectPresentationDefinitions.Borders.ToDictionary(border => border,
            border => document.BorderAnchors[FileSelectParts.BorderName(border)]);
        dynamicAnchors = FileSelectPresentationDefinitions.Anchors.ToDictionary(anchor => anchor,
            anchor => document.DynamicAnchors[FileSelectParts.AnchorName(anchor)]);
        this.document = document;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public int CursorFrameDuration => document.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public int HelmetFrameDuration => document.HelmetFrameDuration;

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
