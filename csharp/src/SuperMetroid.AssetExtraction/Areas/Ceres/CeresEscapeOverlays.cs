using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Editable tiles for Ceres's five fixed warning overlays.</summary>
public sealed class CeresEscapeOverlays
{

    /// <summary>Every page's installed tiles in tilemap order, keyed by the page's installed name.</summary>
    private readonly Dictionary<string, BackgroundTile[]> pages;
    /// <summary>Retains the five complete installed warning pages.</summary>
    private CeresEscapeOverlays(Dictionary<string, BackgroundTile[]> pages) => this.pages = pages;

    /// <summary>One page's installed tiles, in tilemap order.</summary>
    internal BackgroundTile[] PageTiles(CeresEscapeWarningPicture page) => pages[page.Name];
    /// <summary>Validates the five-page JSON document and keeps every page's tiles.</summary>
    internal static CeresEscapeOverlays Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresEscapeOverlayTilemapDocument document = JsonAssetDocument.Read<CeresEscapeOverlayTilemapDocument>(
            json, Options, "Ceres escape overlay tilemap");
        return new(Compile(document));
    }

    /// <summary>Validates and serializes an editable Ceres warning document as camel-case UTF-8 JSON.</summary>
    internal static byte[] Write(CeresEscapeOverlayTilemapDocument document)
    {
        _ = Compile(document);
        return JsonSerializer.SerializeToUtf8Bytes(document, Options);
    }

    /// <summary>Provides a page's tiles only when <paramref name="page"/> is one of the five overlays.</summary>
    /// <param name="page">The overlay.</param>
    /// <param name="tiles">Its tiles, or empty when it is not one of the overlays.</param>
    internal bool TryResolve(CeresEscapeWarningPicture page, out ReadOnlyMemory<BackgroundTile> tiles)
    {
        if (CeresEscapeOverlayPages.All.Contains(page))
        {
            tiles = PageTiles(page);
            return true;
        }
        tiles = default;
        return false;
    }

    /// <summary>Requires the exact five page names and dimensions and copies every page's tiles.</summary>
    private static Dictionary<string, BackgroundTile[]> Compile(
        CeresEscapeOverlayTilemapDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != CeresWarningGlyphDefinitions.Version ||
            document.Pages is null ||
            document.Pages.Count != CeresEscapeOverlayPages.All.Length)
            throw new InvalidDataException(
                $"Ceres escape overlay requires version {CeresWarningGlyphDefinitions.Version} and five named tilemaps.");
        var compiled = new Dictionary<string, BackgroundTile[]>(StringComparer.Ordinal);
        foreach (CeresEscapeWarningPicture definition in
                 CeresEscapeOverlayPages.All)
        {
            if (!document.Pages.TryGetValue(definition.Name, out BackgroundTile[]? tiles) ||
                tiles is null || tiles.Length != definition.WordCount)
                throw new InvalidDataException(
                    $"Ceres escape tilemap {definition.Name} requires {definition.WordCount} tiles.");
            compiled.Add(definition.Name, [.. tiles]);
        }
        if (document.Pages.Keys.Any(name =>
                !CeresEscapeOverlayPages.All
                    .Any(definition => definition.Name == name)))
            throw new InvalidDataException(
                "Ceres escape overlay contains an unknown tilemap name.");
        return compiled;
    }

    /// <summary>Produces readable camel-case documents and rejects unknown JSON properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
