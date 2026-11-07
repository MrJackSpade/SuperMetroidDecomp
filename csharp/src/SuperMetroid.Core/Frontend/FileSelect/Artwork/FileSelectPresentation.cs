using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Immutable file-select layout the game binds: each slot field's anchors, the copy and clear pages' slot-letter anchors,
/// the cursor and helmet anchors and the frame durations. The pages, field patches and glyphs are pictures the import
/// composes once (#1467); the actors are drawn from the installed file-select sheet, whose compositions belong to the
/// import pipeline that reads the installed presentation (#1463). Save-file validation, menu navigation, copy/clear
/// operations and fades remain compiled.
/// </summary>
public abstract class FileSelectPresentation
{
    /// <summary>The slot fields, anchors and durations the import pipeline validated.</summary>
    private readonly FileSelectPlacements placements;

    /// <summary>Keeps the layout's anchors and durations.</summary>
    /// <param name="placements">The validated layout.</param>
    internal FileSelectPresentation(FileSelectPlacements placements) => this.placements = placements;

    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public int CursorFrameDuration => placements.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public int HelmetFrameDuration => placements.HelmetFrameDuration;

    /// <summary>Selects the main or data-management field anchors for one save slot.</summary>
    internal FileSelectSlotFieldDocument Slot(bool dataManagement, int slot)
    {
        FileSelectSlotFieldDocument[] layouts = dataManagement
            ? placements.DataSlots
            : placements.MainSlots;
        return (uint)slot < layouts.Length
            ? layouts[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot));
    }

    /// <summary>Resolves a named dynamic field location in the menu tilemap.</summary>
    internal Position16<TilemapCells> DynamicAnchor(FileSelectAnchor anchor) => placements.DynamicAnchors[anchor];

    /// <summary>Selects the cursor anchor from main, confirmation, or data-management layouts.</summary>
    internal Position16<SpritePixels> CursorPosition(FileSelectCursorPage page, int selected) =>
        Point(page switch
        {
            FileSelectCursorPage.Main => placements.MainCursorAnchors,
            FileSelectCursorPage.DataManagement => placements.DataCursorAnchors,
            FileSelectCursorPage.Confirmation => placements.ConfirmationCursorAnchors,
            _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectCursorPage)} {page}."),
        }, selected, "cursor");

    /// <summary>Draws the page's border frame of the file-select sheet at its authored screen anchor.</summary>
    internal void DrawBorder(ScreenSprites screenSprites, SpriteSheet sheet, FileSelectBorder border) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.BorderSpriteName(border)), placements.BorderAnchors[border], shading: PictureShading.Standard);

    /// <summary>Draws one missile-cursor frame of the file-select sheet.</summary>
    internal static void DrawCursor(ScreenSprites screenSprites, SpriteSheet sheet, int frame, Position16<SpritePixels> anchor) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.CursorSpriteName(frame)), anchor, shading: PictureShading.Standard);

    /// <summary>Draws a selected-slot helmet frame of the file-select sheet at that slot's screen anchor.</summary>
    internal void DrawHelmet(ScreenSprites screenSprites, SpriteSheet sheet, int frame, int slot) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.HelmetSpriteName(frame)),
            Point(placements.HelmetAnchors, slot, "helmet"), shading: PictureShading.Standard);

    /// <summary>Reads an indexed screen anchor, rejecting selections outside its layout.</summary>
    private static Position16<SpritePixels> Point(Position16<SpritePixels>[] points, int index, string owner) =>
        (uint)index < points.Length ? points[index] :
            throw new ArgumentOutOfRangeException(owner);
}
