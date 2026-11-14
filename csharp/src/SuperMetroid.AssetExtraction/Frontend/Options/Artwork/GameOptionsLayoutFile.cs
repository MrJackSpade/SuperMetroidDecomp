using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
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
        GameOptionsPresentationDocument document = JsonAssetDocument.Read<GameOptionsPresentationDocument>(artwork, ArtworkJson.Options)
            ?? throw new InvalidDataException("Options presentation document is null.");
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
        try { document = JsonAssetDocument.Read<GameOptionsPlacementDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Options layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid options layout JSON.", error); }
        if (document.Version != Version)
            throw new InvalidDataException($"Options layout version must be {Version}.");
        return Presentation(document.HeadingAnchors, document.CursorAnchors, document.CursorFrameDuration);
    }

    /// <summary>The presentation the anchors and duration name, once each is checked.</summary>
    /// <exception cref="InvalidDataException">A menu's anchors are missing or off screen, or the duration is below one update.</exception>
    internal static GameOptionsPresentation Presentation(Dictionary<string, Position16<SpritePixels>>? headingAnchors,
        Dictionary<string, Position16<SpritePixels>[]>? cursorAnchors, int cursorFrameDuration)
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
            int expected = GameOptionsNames.CursorCount(page);
            Position16<SpritePixels>[]? points = cursorAnchors[name];
            if (points is null || points.Length != expected)
                throw new InvalidDataException($"Options {name} requires {expected} cursor anchors.");
            foreach (Position16<SpritePixels> point in points)
                ValidatePoint(point, $"{name} cursor", byte.MaxValue);
        }
        return new GameOptionsPresentation(new GameOptionsPlacements
        {
            HeadingAnchors = GameOptionsPresentationFormat.MenuPages.ToDictionary(
                page => page, page => headingAnchors[GameOptionsPresentationFormat.MenuName(page)]),
            CursorAnchors = GameOptionsPresentationFormat.MenuPages.ToDictionary(
                page => page, page => cursorAnchors[GameOptionsPresentationFormat.MenuName(page)]),
            CursorFrameDuration = cursorFrameDuration,
        });
    }

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
    private static void ValidatePoint(Position16<SpritePixels> point, string owner, int maximumX)
    {
        if (point.X < 0 || point.X > maximumX || (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException($"Options {owner} requires X=0..{maximumX} and Y=0..255.");
    }
}
