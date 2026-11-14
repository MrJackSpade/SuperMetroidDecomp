using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Immutable options-screen anchors the game binds. The pages, controller labels, language highlights and toggle choices
/// are pictures the import composes once and the game places by name (#1467); the heading and cursor are drawn from the
/// installed options sheet, whose compositions belong to the import pipeline that reads the installed presentation and
/// never reach the game (#1463, #1616). Navigation, binding swaps, toggles and fades remain application mechanics.
/// </summary>
public sealed class GameOptionsPresentation
{
    /// <summary>The installed heading anchors by menu layout.</summary>
    private readonly Dictionary<GameOptionsPage, Position16<SpritePixels>> headingAnchors;
    /// <summary>The installed cursor anchors by menu layout.</summary>
    private readonly Dictionary<GameOptionsPage, Position16<SpritePixels>[]> cursorAnchors;

    /// <summary>Keeps the complete installed anchors; the installation is their only source (#1596).</summary>
    /// <param name="placements">The validated anchors and cursor duration.</param>
    internal GameOptionsPresentation(GameOptionsPlacements placements)
    {
        Ensure.NotNull(placements);
        headingAnchors = placements.HeadingAnchors.ToDictionary(pair => pair.Key, pair => pair.Value);
        cursorAnchors = placements.CursorAnchors.ToDictionary(pair => pair.Key, pair => (Position16<SpritePixels>[])[.. pair.Value]);
        CursorFrameDuration = placements.CursorFrameDuration;
    }

    /// <summary>Menu updates per frame of the four-frame looping missile cursor, at least one.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Gets the cursor origin for an authored page and selection row.</summary>
    internal Position16<SpritePixels> CursorPosition(GameOptionsPage page, int selectedItem)
    {
        if (!Enum.IsDefined(page))
            throw new ArgumentOutOfRangeException(nameof(page), page, "Options cursors exist only for the three menu pages.");
        Position16<SpritePixels>[] points = cursorAnchors[page];
        return (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException($"Options cursor {page}[{selectedItem}] is not authored.");
    }
    /// <summary>Draws a page heading at its authored origin after applying menu scroll.</summary>
    internal void DrawHeading(ScreenSprites sprites, SpriteSheet sheet, GameOptionsPage page, int verticalScroll)
    {
        Position16<SpritePixels> point = headingAnchors[page];
        // The authored X is validated on screen; scrolling the heading wraps its Y.
        Position16<SpritePixels> origin = new(checked(point.X), CoordinateAxis.Wrap(point.Y - verticalScroll));
        sprites.AddImage(sheet.Frame(GameOptionsNames.HeadingSpriteName(page)), origin, shading: PictureShading.Standard);
    }

    /// <summary>Draws one frame of the missile cursor at the selected position.</summary>
    internal static void DrawCursor(ScreenSprites sprites, SpriteSheet sheet, int frame, Position16<SpritePixels> point) =>
        sprites.AddImage(sheet.Frame(GameOptionsNames.CursorSpriteName(frame)), point, shading: PictureShading.Standard);
}
