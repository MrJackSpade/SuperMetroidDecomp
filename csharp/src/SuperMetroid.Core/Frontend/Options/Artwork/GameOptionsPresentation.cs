using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Immutable options-screen anchors the game binds. The pages, controller labels, language highlights and toggle choices
/// are pictures the import composes once and the game places by name (#1467); the heading and cursor are drawn from the
/// installed options sheet, whose compositions belong to the import pipeline that reads the installed presentation and
/// never reach the game (#1463, #1616). Navigation, binding swaps, toggles and fades remain application mechanics.
/// </summary>
public sealed partial class GameOptionsPresentation
{
    /// <summary>The installed heading anchors by menu layout.</summary>
    private readonly Dictionary<GameOptionsPage, Offset16<ScreenPixels>> headingAnchors;
    /// <summary>The installed cursor anchors by row.</summary>
    private readonly Dictionary<GameOptionsRow, Offset16<ScreenPixels>> cursorAnchors;

    /// <summary>Keeps the complete installed anchors; the installation is their only source (#1596).</summary>
    /// <param name="placements">The validated anchors and cursor duration.</param>
    internal GameOptionsPresentation(GameOptionsPlacements placements)
    {
        Ensure.NotNull(placements);
        headingAnchors = placements.HeadingAnchors.ToDictionary(pair => pair.Key, pair => pair.Value);
        cursorAnchors = placements.CursorAnchors.ToDictionary(pair => pair.Key, pair => pair.Value);
        CursorFrameDuration = placements.CursorFrameDuration;
    }

    /// <summary>Menu updates per frame of the looping missile cursor, at least one.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>The cursor origin on <paramref name="row"/>: the row's installed anchor (#2206, #2415).</summary>
    internal Offset16<ScreenPixels> CursorPosition(GameOptionsRow row) => Ensure.Listed(cursorAnchors, Ensure.NotNull(row));

    /// <summary>Draws a page heading at its authored origin after applying menu scroll.</summary>
    internal void DrawHeading(ScreenSprites sprites, SpriteSheet sheet, GameOptionsPage page, int verticalScroll)
    {
        Offset16<ScreenPixels> point = headingAnchors[page];
        // Scrolling moves the heading up by the menu's scroll.
        Offset16<ScreenPixels> origin = point + new Offset16<ScreenPixels>(0, -verticalScroll);
        sprites.AddImage(sheet.Frame(GameOptionsNames.HeadingSpriteName(page)), origin, shading: PictureShading.Standard);
    }

    /// <summary>Draws one frame of the missile cursor at the selected position.</summary>
    internal static void DrawCursor(ScreenSprites sprites, SpriteSheet sheet, MenuMissileFrame frame, Offset16<ScreenPixels> point) =>
        sprites.AddImage(sheet.Frame(MenuMissileFrames.Name(frame)), point, shading: PictureShading.Standard);
}
