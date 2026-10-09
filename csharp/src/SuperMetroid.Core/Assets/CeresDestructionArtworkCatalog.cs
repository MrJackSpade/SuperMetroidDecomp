using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable map and character transfers unique to the destruction of Ceres and
/// the following Zebes reveal. The Ceres character sheets and the first two map
/// slices are shared with <see cref="CeresFlightArtworkCatalog"/>.
/// </summary>
public sealed class CeresDestructionArtworkCatalog
{
    /// <summary>Stores the validated Ceres map bytes and compiled Zebes reveal and destruction presentation assets.</summary>
    /// <param name="ceresMaps">Three ordered 32-by-24 Mode 7 map slices compiled as bytes.</param>
    /// <param name="zebesMap">Compiled BG tilemap page used for the Zebes reveal.</param>
    /// <param name="zebesCharacters">Compiled 4bpp character transfer for the reveal.</param>
    /// <param name="sprites">Validated OAM compositions for the destruction and reveal scenes.</param>
    /// <param name="revealActors">Validated initial placements for the six reveal actors.</param>
    /// <param name="destructionActors">Validated initial placements for the three destruction-background actors.</param>
    private CeresDestructionArtworkCatalog(byte[] ceresMaps,
        RoomBackgroundTilemapAtlas zebesMap, RoomCharacterAtlas zebesCharacters,
        CeresDestructionSpritePresentation sprites, CeresRevealActorLayout revealActors,
        CeresDestructionActorLayout destructionActors)
    {
        CeresMaps = ceresMaps;
        ZebesMap = zebesMap;
        ZebesCharacters = zebesCharacters;
        Sprites = sprites;
        RevealActors = revealActors;
        DestructionActors = destructionActors;
    }

    /// <summary>Two destruction views followed by the native clear-map slice.</summary>
    public ReadOnlyMemory<byte> CeresMaps { get; }
    /// <summary>One compiled 32-by-32 ordinary BG tilemap page for the Zebes reveal, replacing the $0800-byte map imported from $97:8ADB and transferred by $8B:C6F1–$C70E.</summary>
    public RoomBackgroundTilemapAtlas ZebesMap { get; }
    /// <summary>Compiled $4000-byte 4bpp character sheet for the Zebes reveal, replacing artwork imported from $96:EC76 and loaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas ZebesCharacters { get; }
    /// <summary>Selected OAM compositions unique to the station explosion and Zebes reveal; approach sprites remain shared with the Ceres-flight catalog.</summary>
    public CeresDestructionSpritePresentation Sprites { get; }
    /// <summary>Initial screen-pixel placements of the six Zebes-reveal actors; instruction lists, slide acceleration, deletion, and phase transitions remain compiled cinematic behavior.</summary>
    public CeresRevealActorLayout RevealActors { get; }
    /// <summary>Initial screen-pixel placements of the three persistent destruction-background actors; movement and wrap behavior are not editable through this layout.</summary>
    public CeresDestructionActorLayout DestructionActors { get; }

    /// <summary>Identity of all selected destruction/reveal maps, characters, OAM frames and placements.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresDestructionArtworkCatalog), content =>
    {
        content.Append("ceres-maps", CeresMaps.Span);
        content.Append("zebes-map", ZebesMap.Transfer.Span);
        content.Append("zebes-characters", ZebesCharacters.Transfer.Span);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("reveal-actors", Convert.FromHexString(RevealActors.ContentIdentity));
        content.Append("destruction-actors", Convert.FromHexString(DestructionActors.ContentIdentity));
    });

    /// <summary>Loads the bounded destruction and Zebes-reveal presentation resources, validating each map, character sheet, sprite catalog, and actor layout without reading cartridge data.</summary>
    /// <param name="ceresMapJson">Version-1 three-view Mode 7 map JSON from <c>ceres-destruction-mode7-maps.json</c>.</param>
    /// <param name="zebesMapJson">One-page BG tilemap JSON from <c>zebes-reveal-tilemap.json</c>.</param>
    /// <param name="zebesCharactersPng">Indexed 4bpp character artwork from <c>zebes-reveal-characters.png</c>, compiling to $4000 bytes.</param>
    /// <param name="spritesJson">Named station-blast and reveal OAM compositions.</param>
    /// <param name="revealActorsJson">Six ordered initial Zebes-reveal actor placements.</param>
    /// <param name="destructionActorsJson">Three ordered initial destruction-background actor placements.</param>
    /// <returns>The compiled presentation; all input streams are consumed from their current positions and remain caller-owned.</returns>
    public static CeresDestructionArtworkCatalog Load(Stream ceresMapJson,
        Stream zebesMapJson, Stream zebesCharactersPng, Stream spritesJson,
        Stream revealActorsJson, Stream destructionActorsJson)
    {
        ArgumentNullException.ThrowIfNull(ceresMapJson);
        ArgumentNullException.ThrowIfNull(zebesMapJson);
        ArgumentNullException.ThrowIfNull(zebesCharactersPng);
        ArgumentNullException.ThrowIfNull(spritesJson);
        ArgumentNullException.ThrowIfNull(revealActorsJson);
        ArgumentNullException.ThrowIfNull(destructionActorsJson);
        CeresDestructionMapDocument document = ReadMap(ceresMapJson);
        var maps = new byte[CeresDestructionArtworkFormat.MapByteCount];
        for (int view = 0; view < document.Views.Length; view++)
            for (int cell = 0; cell < CeresDestructionArtworkFormat.CellsPerView; cell++)
                maps[view * CeresDestructionArtworkFormat.CellsPerView + cell] =
                    checked((byte)document.Views[view][cell]);
        return new CeresDestructionArtworkCatalog(maps,
            RoomBackgroundTilemapAtlas.Load(zebesMapJson,
                CeresDestructionArtworkFormat.ZebesMapByteCount),
            RoomCharacterAtlas.Load(zebesCharactersPng,
                CeresDestructionArtworkFormat.ZebesCharacterByteCount),
            CeresDestructionSpritePresentation.Load(spritesJson),
            CeresRevealActorLayout.Load(revealActorsJson),
            CeresDestructionActorLayout.Load(destructionActorsJson));
    }

    /// <summary>Serializes the three-view Mode 7 map as UTF-8 JSON, validating version, dimensions, cell counts, and eight-bit tile indexes before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Ordered destruction-view and clear-view cells to serialize.</param>
    public static void WriteMap(Stream json, CeresDestructionMapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Deserializes and validates the three ordered Mode 7 views in a Ceres destruction map document.</summary>
    /// <param name="json">Source stream positioned at the beginning of the JSON document.</param>
    /// <returns>The validated version-1 document with 32-by-24 eight-bit tile-index arrays.</returns>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, or violates the required schema or tile-index range.</exception>
    private static CeresDestructionMapDocument ReadMap(Stream json)
    {
        CeresDestructionMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDestructionMapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres destruction map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction map JSON.", error);
        }
        if (document.Version != CeresDestructionArtworkFormat.Version ||
            document.Width != CeresDestructionArtworkFormat.MapWidth ||
            document.Height != CeresDestructionArtworkFormat.MapHeight ||
            document.Views is not { Length: CeresDestructionArtworkFormat.ViewCount } ||
            document.Views.Any(view => view is not
                { Length: CeresDestructionArtworkFormat.CellsPerView } ||
                view.Any(tile => (uint)tile > byte.MaxValue)))
            throw new InvalidDataException(
                "Ceres destruction requires three ordered 32x24 arrays of eight-bit tile indexes.");
        return document;
    }
}

/// <summary>Editable three-slice Mode 7 map schema for Ceres destruction; each cell is an eight-bit character index without ordinary BG palette, priority, or flip attributes.</summary>
public sealed record CeresDestructionMapDocument
{
    /// <summary>Map schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Required width of each row-major view in Mode 7 map cells: 32.</summary>
    public required int Width { get; init; }
    /// <summary>Required height of each row-major view in Mode 7 map cells: 24.</summary>
    public required int Height { get; init; }
    /// <summary>Native source offsets $600, $900 and $C00, in that order.</summary>
    public required int[][] Views { get; init; }
}

/// <summary>Files and exact native transfer dimensions for the destruction/reveal art.</summary>
public static class CeresDestructionArtworkFormat
{
    /// <summary>Supported schema revision for the Ceres destruction Mode 7 map document.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for the two destruction map views followed by the native clear-map slice.</summary>
    public const string CeresMapFileName = "ceres-destruction-mode7-maps.json";
    /// <summary>Installed JSON filename for the ordinary 32-by-32 BG tilemap page shown during the Zebes reveal.</summary>
    public const string ZebesMapFileName = "zebes-reveal-tilemap.json";
    /// <summary>Installed indexed PNG filename for the Zebes-reveal 4bpp characters, separate from its tilemap references.</summary>
    public const string ZebesCharacterFileName = "zebes-reveal-characters.png";
    /// <summary>Number of eight-bit Mode 7 map cells per row in each exported Ceres slice.</summary>
    public const int MapWidth = 32;
    /// <summary>Number of map-cell rows in each exported Ceres slice.</summary>
    public const int MapHeight = 24;
    /// <summary>768 row-major cells per Ceres view, each compiling to one map-lane byte.</summary>
    public const int CellsPerView = MapWidth * MapHeight;
    /// <summary>Three ordered Ceres slices from decompressed source offsets $600, $900, and $C00; the last clears the scene.</summary>
    public const int ViewCount = 3;
    /// <summary>$0900 compiled map-lane bytes for all three slices, appended after the shared $0600-byte Ceres-flight maps.</summary>
    public const int MapByteCount = ViewCount * CellsPerView;
    /// <summary>$0800 bytes for one ordinary BG tilemap page, transferred from native ZebesTilemap to VRAM word $5C00 by $8B:C6F1–$C70E.</summary>
    public const int ZebesMapByteCount = 0x0800;
    /// <summary>$4000 bytes of 4bpp Zebes characters, matching the native ZebesTiles transfer to VRAM word $6000 at $8B:C70C–$C72E.</summary>
    public const int ZebesCharacterByteCount = 0x4000;
}
