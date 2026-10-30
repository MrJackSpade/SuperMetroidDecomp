using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Editable tiles for Ceres's five fixed warning overlays.</summary>
public sealed class CeresEscapeOverlays
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-escape-overlay-v2", content =>
    {
        content.Append("shared", ReadOnlySpan<byte>.Empty);
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            content.Append("source", Encoding.UTF8.GetBytes(page.Name));
            content.AppendTiles("tiles", PageTiles(page));
        }
    });

    /// <summary>Selected tiles differing from the calculated stock warning pages, keyed by page and position.</summary>
    internal readonly Dictionary<CeresEscapeOverlayWord, BackgroundTile> edits;
    /// <summary>Retains only authored deviations from the stock Ceres warning tilemaps.</summary>
    private CeresEscapeOverlays(Dictionary<CeresEscapeOverlayWord, BackgroundTile> edits) => this.edits = edits;

    /// <summary>One page's calculated and edited tiles, in tilemap order.</summary>
    private BackgroundTile[] PageTiles(CeresEscapeOverlayTilemapDefinition page)
    {
        var tiles = new BackgroundTile[page.WordCount];
        for (int index = 0; index < page.WordCount; index++)
        {
            tiles[index] = edits.GetValueOrDefault(
                new CeresEscapeOverlayWord(page.Name, index), CeresEscapeWarningGlyphs.StockTile(page, index));
        }
        return tiles;
    }
    /// <summary>Validates the five-page JSON document and compiles its edited tiles.</summary>
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
    internal bool TryResolve(CeresEscapeOverlayTilemapDefinition page, out ReadOnlyMemory<BackgroundTile> tiles)
    {
        if (CeresEscapeOverlayTilemapDefinitions.All.Contains(page))
        {
            tiles = PageTiles(page);
            return true;
        }
        tiles = default;
        return false;
    }

    /// <summary>Requires the exact five page names and dimensions while recording only tiles that differ from stock.</summary>
    private static Dictionary<CeresEscapeOverlayWord, BackgroundTile> Compile(
        CeresEscapeOverlayTilemapDocument document)
    {
        Ensure.NotNull(document);
        if (document.Version != CeresWarningGlyphDefinitions.Version ||
            document.Pages is null ||
            document.Pages.Count != CeresEscapeOverlayTilemapDefinitions.All.Length)
            throw new InvalidDataException(
                $"Ceres escape overlay requires version {CeresWarningGlyphDefinitions.Version} and five named tilemaps.");
        var compiled = new Dictionary<CeresEscapeOverlayWord, BackgroundTile>();
        foreach (CeresEscapeOverlayTilemapDefinition definition in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (!document.Pages.TryGetValue(definition.Name, out BackgroundTile[]? tiles) ||
                tiles is null || tiles.Length != definition.WordCount)
                throw new InvalidDataException(
                    $"Ceres escape tilemap {definition.Name} requires {definition.WordCount} tiles.");
            for (int index = 0; index < tiles.Length; index++)
                if (tiles[index] != CeresEscapeWarningGlyphs.StockTile(definition, index))
                    compiled.Add(new CeresEscapeOverlayWord(definition.Name, index), tiles[index]);
        }
        if (document.Pages.Keys.Any(name =>
                !CeresEscapeOverlayTilemapDefinitions.All
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
