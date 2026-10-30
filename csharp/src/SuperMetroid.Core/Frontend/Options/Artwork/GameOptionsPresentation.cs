using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Immutable options-screen anchors the game binds. The pages, controller labels, language highlights and toggle choices
/// are pictures the import composes once and the game places by name (#1467); the heading and cursor are drawn from the
/// installed options sheet, whose compositions belong to the import pipeline that reads the installed presentation
/// (#1463). Navigation, binding swaps, toggles and fades remain application mechanics.
/// </summary>
public abstract class GameOptionsPresentation
{
    /// <summary>Edited heading anchors by menu layout, or null for the native anchors.</summary>
    internal readonly Dictionary<GameOptionsPage, Position16<SpritePixels>>? headingAnchors;
    /// <summary>Edited cursor anchors by menu layout, or null for the native anchors.</summary>
    internal readonly Dictionary<GameOptionsPage, Position16<SpritePixels>[]>? cursorAnchors;

    /// <summary>Retains only the anchor edits that depart from stock geometry.</summary>
    /// <param name="document">The validated layout.</param>
    /// <param name="contentIdentity">The identity of the selected presentation's bytes.</param>
    internal GameOptionsPresentation(GameOptionsLayoutDocument document, string contentIdentity)
    {
        Dictionary<GameOptionsPage, Position16<SpritePixels>> headings = GameOptionsPresentationDefinitions.MenuPages.ToDictionary(
            page => page, page => document.HeadingAnchors[GameOptionsNames.MenuName(page)]);
        headingAnchors = headings.Any(pair => pair.Value != GameOptionsArtworkCells.HeadingAnchor(pair.Key)) ? headings : null;
        Dictionary<GameOptionsPage, Position16<SpritePixels>[]> cursors = GameOptionsPresentationDefinitions.MenuPages.ToDictionary(
            page => page, page => document.CursorAnchors[GameOptionsNames.MenuName(page)]);
        cursorAnchors = cursors.Any(pair => pair.Value.Where((point, index) =>
            point != GameOptionsArtworkCells.CursorAnchor(pair.Key, index)).Any()) ? cursors : null;
        HiddenCursor = document.HiddenCursor;
        CursorPalette = document.CursorPalette;
        CursorFrameDuration = document.CursorFrameDuration;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying selected artwork and layout including their encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Cursor origin used during phases without a cursor table, in screen pixels with X=0-511 and Y=0-255; the actor remains in OAM.</summary>
    public Position16<SpritePixels> HiddenCursor { get; }
    /// <summary>OBJ palette index 0-7 used for both the missile cursor and menu-heading sprite compositions.</summary>
    public int CursorPalette { get; }
    /// <summary>Menu updates per frame of the four-frame looping missile cursor, in the range 1-65535.</summary>
    public int CursorFrameDuration { get; }

    /// <summary>Gets the cursor origin for an authored page and selection row.</summary>
    internal Position16<SpritePixels> CursorPosition(GameOptionsPage page, int selectedItem)
    {
        if (!Enum.IsDefined(page))
            throw new ArgumentOutOfRangeException(nameof(page), page, "Options cursors exist only for the three menu pages.");
        if (cursorAnchors is null)
            return GameOptionsArtworkCells.CursorAnchor(page, selectedItem);
        Position16<SpritePixels>[] points = cursorAnchors[page];
        return (uint)selectedItem < points.Length
            ? points[selectedItem]
            : throw new InvalidDataException($"Options cursor {page}[{selectedItem}] is not authored.");
    }
    /// <summary>Draws a page heading at its authored origin after applying menu scroll.</summary>
    internal void DrawHeading(ScreenSprites sprites, SpriteSheet sheet, GameOptionsPage page, int verticalScroll)
    {
        Position16<SpritePixels> point = headingAnchors is null
            ? GameOptionsArtworkCells.HeadingAnchor(page)
            : headingAnchors[page];
        // The authored X is validated on screen; scrolling the heading wraps its Y word.
        Position16<SpritePixels> origin = new(checked(point.X), unchecked((ushort)(point.Y - verticalScroll)));
        sprites.AddImage(sheet.Frame(GameOptionsNames.HeadingSpriteName(page)), origin);
    }

    /// <summary>Draws one frame of the missile cursor at the selected position.</summary>
    internal static void DrawCursor(ScreenSprites sprites, SpriteSheet sheet, int frame, Position16<SpritePixels> point) =>
        sprites.AddImage(sheet.Frame(GameOptionsNames.CursorSpriteName(frame)), point);
}
