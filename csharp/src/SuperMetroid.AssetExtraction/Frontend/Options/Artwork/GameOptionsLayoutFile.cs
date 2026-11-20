using System.Text.Json;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// The installed options-menu layout (#1638): the heading and cursor anchors and the cursor's timing the game binds. The pages,
/// labels, highlight regions, compositions and palette selectors, whose entries are the original's packed tile and sprite
/// placements, are import data (<see cref="GameOptionsPresentationFormat.ArtworkFileName"/>).
/// </summary>
public static class GameOptionsLayoutFile
{
    /// <summary>Supported layout revision: 2 holds no hidden cursor anchor, since transitions show no cursor (#1775).</summary>
    public const int Version = 2;

    /// <summary>The layout of the extracted artwork document <paramref name="artwork"/>, as the installation holds it.</summary>
    /// <exception cref="InvalidDataException">The artwork document does not decode.</exception>
    internal static byte[] Extract(byte[] artwork)
    {
        GameOptionsPresentationDocument document = JsonAssetDocument.Read<GameOptionsPresentationDocument>(artwork, ArtworkJson.Options);
        return JsonSerializer.SerializeToUtf8Bytes(new GameOptionsPlacementDocument
        {
            Version = Version, HeadingAnchors = document.HeadingAnchors, CursorAnchors = document.CursorAnchors,
            CursorFrameDuration = document.CursorFrameDuration,
        }, ArtworkJson.Options);
    }

    /// <summary>Reads and validates the installed layout the game binds.</summary>
    /// <exception cref="InvalidDataException">The JSON, version, anchors or duration are invalid.</exception>
    public static GameOptionsPresentation Read(Stream json)
    {
        GameOptionsPlacementDocument document;
        try { document = JsonAssetDocument.Read<GameOptionsPlacementDocument>(json, ArtworkJson.Options); }
        catch (JsonException error) { throw new InvalidDataException("Invalid options layout JSON.", error); }
        if (document.Version != Version)
            throw new InvalidDataException($"Options layout version must be {Version}.");
        return Presentation(document.HeadingAnchors, document.CursorAnchors, document.CursorFrameDuration);
    }

    /// <summary>The presentation the anchors and duration name, once each is checked.</summary>
    /// <remarks>
    /// The layout lists each page's cursor anchors in its rows' order from the top, which keys them by row (#2415).
    /// </remarks>
    /// <exception cref="InvalidDataException">A menu's anchors are missing or off screen, or the duration is below one update.</exception>
    internal static GameOptionsPresentation Presentation(Dictionary<string, Offset16<ScreenPixels>>? headingAnchors,
        Dictionary<string, Offset16<ScreenPixels>[]>? cursorAnchors, int cursorFrameDuration)
    {
        string[] menuNames = [.. GameOptionsPresentationFormat.MenuPages.Select(GameOptionsPresentationFormat.MenuName)];
        RequireExactKeys(headingAnchors, menuNames, "options heading anchors");
        RequireExactKeys(cursorAnchors, menuNames, "options cursor anchors");
        if (cursorFrameDuration < 1)
            throw new InvalidDataException("Options cursor frame duration must be at least one update.");
        foreach (GameOptionsPage page in GameOptionsPresentationFormat.MenuPages)
        {
            string name = GameOptionsPresentationFormat.MenuName(page);
            ValidatePoint(headingAnchors[name], $"{name} heading", byte.MaxValue);
            int expected = RowsOn(page).Count;
            Offset16<ScreenPixels>[]? points = cursorAnchors[name];
            if (points is null || points.Length != expected)
                throw new InvalidDataException($"Options {name} requires {expected} cursor anchors.");
            foreach (Offset16<ScreenPixels> point in points)
                ValidatePoint(point, $"{name} cursor", byte.MaxValue);
        }
        return new GameOptionsPresentation(new GameOptionsPlacements
        {
            HeadingAnchors = GameOptionsPresentationFormat.MenuPages.ToDictionary(
                page => page, page => headingAnchors[GameOptionsPresentationFormat.MenuName(page)]),
            CursorAnchors = CursorAnchorsByRow(cursorAnchors),
            CursorFrameDuration = cursorFrameDuration,
        });
    }

    /// <summary>Each row's cursor anchor, from the checked lists of each page's anchors in its rows' order from the top.</summary>
    private static Dictionary<GameOptionsRow, Offset16<ScreenPixels>> CursorAnchorsByRow(
        Dictionary<string, Offset16<ScreenPixels>[]> cursorAnchors)
    {
        var byRow = new Dictionary<GameOptionsRow, Offset16<ScreenPixels>>();
        foreach (GameOptionsPage page in GameOptionsPresentationFormat.MenuPages)
        {
            IReadOnlyList<GameOptionsRow> rows = RowsOn(page);
            Offset16<ScreenPixels>[] points = cursorAnchors[GameOptionsPresentationFormat.MenuName(page)];
            for (int place = 0; place < rows.Count; place++)
                byRow.Add(rows[place], points[place]);
        }
        return byRow;
    }

    /// <summary>The rows of <paramref name="page"/> from the top, the order the layout lists the page's cursor anchors in.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not one of the three options pages.</exception>
    private static IReadOnlyList<GameOptionsRow> RowsOn(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => [.. GameOptionsPageRows.Primary.Select(row => new GameOptionsRow.OnPrimaryPage(row))],
        GameOptionsPage.Controller => [.. GameOptionsPageRows.Controller.Select(row => new GameOptionsRow.OnControllerPage(row))],
        GameOptionsPage.Special => [.. GameOptionsPageRows.Special.Select(row => new GameOptionsRow.OnSpecialPage(row))],
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Options rows exist only on the three menu pages."),
    };

    /// <summary>Requires exactly the names <paramref name="expected"/> in <paramref name="values"/>.</summary>
    /// <exception cref="InvalidDataException">A name is missing or another is present.</exception>
    private static void RequireExactKeys<T>([System.Diagnostics.CodeAnalysis.NotNull] Dictionary<string, T>? values, string[] expected, string owner)
    {
        if (values is null || values.Count != expected.Length)
            throw new InvalidDataException($"{owner} requires exactly {expected.Length} named entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    /// <summary>Requires a screen origin within X=0..<paramref name="maximumX"/> and Y=0..255.</summary>
    /// <exception cref="InvalidDataException">The origin is outside that range.</exception>
    private static void ValidatePoint(Offset16<ScreenPixels> point, string owner, int maximumX)
    {
        if (point.X < 0 || point.X > maximumX || (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException($"Options {owner} requires X=0..{maximumX} and Y=0..255.");
    }
}
