using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable map and character transfers unique to the destruction of Ceres and
/// the following Zebes reveal. The Ceres character sheets and the first two map
/// slices are shared with <see cref="CeresFlightArtworkCatalog"/>.
/// </summary>
public sealed class CeresDestructionArtworkCatalog
{
    private CeresDestructionArtworkCatalog(byte[] ceresMaps,
        RoomBackgroundTilemapAtlas zebesMap, RoomCharacterAtlas zebesCharacters,
        CeresDestructionSpritePresentation sprites)
    {
        CeresMaps = ceresMaps;
        ZebesMap = zebesMap;
        ZebesCharacters = zebesCharacters;
        Sprites = sprites;
    }

    /// <summary>Two destruction views followed by the native clear-map slice.</summary>
    public ReadOnlyMemory<byte> CeresMaps { get; }
    public RoomBackgroundTilemapAtlas ZebesMap { get; }
    public RoomCharacterAtlas ZebesCharacters { get; }
    public CeresDestructionSpritePresentation Sprites { get; }

    public static CeresDestructionArtworkCatalog Load(Stream ceresMapJson,
        Stream zebesMapJson, Stream zebesCharactersPng, Stream spritesJson)
    {
        ArgumentNullException.ThrowIfNull(ceresMapJson);
        ArgumentNullException.ThrowIfNull(zebesMapJson);
        ArgumentNullException.ThrowIfNull(zebesCharactersPng);
        ArgumentNullException.ThrowIfNull(spritesJson);
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
            CeresDestructionSpritePresentation.Load(spritesJson));
    }

    public static void WriteMap(Stream json, CeresDestructionMapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

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

public sealed record CeresDestructionMapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    /// <summary>Native source offsets $600, $900 and $C00, in that order.</summary>
    public required int[][] Views { get; init; }
}

/// <summary>Files and exact native transfer dimensions for the destruction/reveal art.</summary>
public static class CeresDestructionArtworkFormat
{
    public const int Version = 1;
    public const string CeresMapFileName = "ceres-destruction-mode7-maps.json";
    public const string ZebesMapFileName = "zebes-reveal-tilemap.json";
    public const string ZebesCharacterFileName = "zebes-reveal-characters.png";
    public const int MapWidth = 32;
    public const int MapHeight = 24;
    public const int CellsPerView = MapWidth * MapHeight;
    public const int ViewCount = 3;
    public const int MapByteCount = ViewCount * CellsPerView;
    public const int ZebesMapByteCount = 0x0800;
    public const int ZebesCharacterByteCount = 0x4000;
}
