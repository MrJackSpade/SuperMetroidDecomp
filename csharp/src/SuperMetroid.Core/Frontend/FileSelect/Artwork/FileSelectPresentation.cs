using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Immutable file-select layout the game binds: each slot field's anchors, the copy and clear pages' slot-letter anchors,
/// the cursor and helmet anchors and the frame durations. The pages, field patches and glyphs are pictures the import
/// composes once (#1467); the actors are drawn from the installed file-select sheet, whose compositions belong to the
/// import pipeline that reads the installed presentation and never reach the game (#1463, #1616). Save-file validation,
/// menu navigation, copy/clear operations and fades are the game's own.
/// </summary>
public sealed partial class FileSelectPresentation
{
    /// <summary>The slot fields, anchors and durations the import pipeline validated.</summary>
    private readonly FileSelectPlacements placements;

    /// <summary>Keeps the layout's anchors and durations.</summary>
    /// <param name="placements">The validated layout.</param>
    internal FileSelectPresentation(FileSelectPlacements placements) => this.placements = placements;

    /// <summary>Menu updates per missile-cursor animation frame, at least one.</summary>
    public int CursorFrameDuration => placements.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, at least one.</summary>
    public int HelmetFrameDuration => placements.HelmetFrameDuration;

    /// <summary>Selects the main or data-management field anchors for one save slot.</summary>
    internal FileSelectSlotFieldDocument Slot(FileSelectCursorPage page, SaveSlot slot) => Ensure.Listed(page switch
    {
        FileSelectCursorPage.Main => placements.MainSlots,
        FileSelectCursorPage.DataManagement => placements.DataSlots,
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined file-select page."),
    }, slot);

    /// <summary>The page pixel a copy or clear prompt's slot letter is shown at.</summary>
    internal Position16<PlanePixels> DynamicAnchor(FileSelectAnchor anchor) => placements.DynamicAnchors[anchor];

    /// <summary>Where the cursor points at each item on each page that shows one.</summary>
    [field: NonSerialized]
    private IdentityTable<FileSelectCursorPage, Func<FileSelectItem, Offset16<ScreenPixels>>> CursorAnchors => field ??= new(new Dictionary<FileSelectCursorPage, Func<FileSelectItem, Offset16<ScreenPixels>>>
    {
        [FileSelectCursorPage.Main] = item => Ensure.Listed(placements.MainCursorAnchors, item),
        [FileSelectCursorPage.DataManagement] = item => Ensure.Listed(placements.DataCursorAnchors, item),
    });

    /// <summary>The cursor anchor at <paramref name="item"/>'s row on the main page or a copy or clear page.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The item is not a row of that page.</exception>
    internal Offset16<ScreenPixels> CursorPosition(FileSelectCursorPage page, FileSelectItem item) => CursorAnchors[page](item);

    /// <summary>The cursor anchor at <paramref name="answer"/>'s row of a copy or clear confirmation.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The answer is undefined.</exception>
    internal Offset16<ScreenPixels> CursorPosition(YesNoChoice answer) => Ensure.Listed(placements.ConfirmationCursorAnchors, answer);

    /// <summary>Draws the page's border frame of the file-select sheet at its authored screen anchor.</summary>
    internal void DrawBorder(ScreenSprites screenSprites, SpriteSheet sheet, FileSelectBorder border) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.BorderSpriteName(border)), placements.BorderAnchors[border], shading: PictureShading.Standard);

    /// <summary>Draws one missile-cursor frame of the file-select sheet.</summary>
    internal static void DrawCursor(ScreenSprites screenSprites, SpriteSheet sheet, MenuMissileFrame frame, Offset16<ScreenPixels> anchor) =>
        screenSprites.AddImage(sheet.Frame(MenuMissileFrames.Name(frame)), anchor, shading: PictureShading.Standard);

    /// <summary>Draws a slot's helmet picture of the file-select sheet at that slot's screen anchor.</summary>
    internal void DrawHelmet(ScreenSprites screenSprites, SpriteSheet sheet, FileSelectHelmetFrame frame, SaveSlot slot) =>
        screenSprites.AddImage(sheet.Frame(FileSelectParts.HelmetSpriteName(frame)),
            Ensure.Listed(placements.HelmetAnchors, slot), shading: PictureShading.Standard);
}
