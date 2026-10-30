using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The installed approach to Ceres as the import pipeline reads it (#1409): its Mode 7 floor characters as chunky 8-bit
/// pixels and its OBJ characters as SNES 4-bpp planar bytes, which the flight sheets compose from, beside the maps, palette
/// and compositions the game binds.
/// </summary>
public sealed class CeresFlightCharacters : CeresFlightArtwork
{
    /// <summary>Retains compiled characters and selected palette, OAM, and actor resources.</summary>
    /// <param name="floorCharacters">The chunky Mode 7 floor characters.</param>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="objectCharacters">The planar 4-bpp OBJ characters.</param>
    /// <param name="palette">The approach palette.</param>
    /// <param name="sprites">The approach's OAM compositions.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    private CeresFlightCharacters(byte[] floorCharacters, byte[] floorMaps,
        byte[] objectCharacters, CeresFlightPalette palette,
        CeresFlightSpritePresentation sprites, CeresFlightActorPlacements actors)
        : base(palette, actors, floorCharacters.Length / CeresFlightArtworkFormat.FloorCharacterBytes,
            objectCharacters.Length / CeresFlightArtworkFormat.ObjectCharacterBytes)
    {
        FloorCharacters = floorCharacters;
        FloorMaps = floorMaps;
        ObjectCharacters = objectCharacters;
        Sprites = sprites;
    }

    /// <summary>Selected six-frame compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction scene's sheets.</summary>
    public CeresFlightSpritePresentation Sprites { get; }

    /// <summary>Owned $4000-byte stream of 256 chunky 8-bpp Mode-7 characters, imported from <c>Tiles_Gunship_Ceres_Mode7</c> at <c>$95:A82F</c>; also reused by the ending flyaway.</summary>
    public ReadOnlyMemory<byte> FloorCharacters { get; }

    /// <summary>Front 768 map bytes followed by rear 768 map bytes, which the import pictures into the flight's floors (#1467).</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }

    /// <summary>Owned $4000-byte stream of 512 planar 4-bpp characters, imported from <c>Tiles_Space_Ceres</c> at <c>$96:D10A</c> and uploaded to VRAM bytes $C000-$FFFF.</summary>
    public ReadOnlyMemory<byte> ObjectCharacters { get; }

    /// <summary>The flight OBJ page the space, gunship and station compositions draw from: the OBJ sheet as $BCCD uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.CeresFlightObjects),
        ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount]);

    /// <inheritdoc/>
    public override string ContentIdentity => SelectedPresentationHash.Create("CeresFlightArtworkCatalog", content =>
    {
        content.Append("mode7-characters", FloorCharacters.Span);
        content.Append("mode7-maps", FloorMaps.Span);
        content.Append("object-characters", ObjectCharacters.Span);
        content.Append("palette", ScreenColourImage.Words(Palette.Colours));
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("actors", Convert.FromHexString(Actors.ContentIdentity));
    });

    /// <summary>Compiles the six selected Ceres approach resources into native character, map, palette, and actor presentation data.</summary>
    /// <param name="floorPng">128-by-128 indexed PNG with 256 palette entries, supplying 256 chunky 8-bpp characters.</param>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="objectPng">256-by-128 indexed PNG with sixteen palette entries, supplying 512 planar 4-bpp characters.</param>
    /// <param name="palette">The approach's 256 selected colours, read from the installed palette document.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <returns>A catalog owning newly compiled transfer bytes and loaded presentation resources.</returns>
    /// <remarks>Reads from the streams' current positions without disposing them. PNG pixel indices define characters; the separate palette resource defines rendered colors. File provenance and stock hashes are validated by installation loading, not by this method.</remarks>
    /// <exception cref="ArgumentNullException">Any resource stream is null.</exception>
    /// <exception cref="InvalidDataException">A resource has an invalid format, dimensions, palette size, schema, map index, actor identity, composition, or compiled transfer length.</exception>
    public static CeresFlightCharacters Read(Stream floorPng, Stream mapJson,
        Stream objectPng, CeresFlightPalette palette, Stream spritesJson, Stream actorsJson)
    {
        Ensure.NotNull(floorPng);
        Ensure.NotNull(mapJson);
        Ensure.NotNull(objectPng);
        Ensure.NotNull(palette);
        Ensure.NotNull(spritesJson);
        Ensure.NotNull(actorsJson);
        IndexedPngImage floorImage = IndexedPng.Read(floorPng,
            new(CeresFlightArtworkFormat.FloorWidth, CeresFlightArtworkFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            new(CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight));
        if (floorImage.Colours.Length != 256 || objects.Colours.Length != 16)
            throw new InvalidDataException("Ceres flight requires 256-color Mode-7 and 16-color OBJ PNGs.");

        byte[] map = ReadMaps(mapJson);
        byte[] characters = SnesMode7TileEncoder.Encode(floorImage.Pixels, floorImage.Size);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Size, 4);
        if (characters.Length != CeresFlightArtworkFormat.FloorByteCount ||
            objectCharacters.Length != CeresFlightArtworkFormat.ObjectByteCount)
            throw new InvalidDataException("Ceres flight PNGs compile to unexpected DMA lengths.");
        return new CeresFlightCharacters(characters, map, objectCharacters,
            palette,
            CeresFlightSpritePresentation.Load(spritesJson),
            CeresFlightActorPlacements.Load(actorsJson));
    }

    /// <summary>The characters <paramref name="flight"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The flight artwork was not read from installed artwork.</exception>
    public static CeresFlightCharacters Of(CeresFlightArtwork flight) => flight as CeresFlightCharacters
        ?? throw new InvalidDataException("The Ceres flight artwork was not read from installed artwork.");

    /// <summary>Reads both ordered 768-byte front and rear map slices from a version-one map document.</summary>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <returns>Front 768 map bytes followed by rear 768 map bytes.</returns>
    /// <exception cref="InvalidDataException">The document has an unsupported version, dimensions, array length, or tile index.</exception>
    internal static byte[] ReadMaps(Stream mapJson)
    {
        CeresFlightMapDocument document = ReadMap(Ensure.NotNull(mapJson));
        byte[] map = new byte[2 * CeresFlightArtworkFormat.MapCellsPerView];
        for (int index = 0; index < CeresFlightArtworkFormat.MapCellsPerView; index++)
        {
            map[index] = checked((byte)document.FrontTiles[index]);
            map[CeresFlightArtworkFormat.MapCellsPerView + index] =
                checked((byte)document.RearTiles[index]);
        }
        return map;
    }

    /// <summary>Serializes a front/rear map document and validates its serialized form before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned writable destination stream, written at its current position.</param>
    /// <param name="document">Version-one document with two complete 32-by-24 editing arrays containing tile indices $00-$FF.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The document is null or has an unsupported version, dimensions, array length, or tile index.</exception>
    public static void WriteMap(Stream json, CeresFlightMapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Requires two complete 32-by-24 tile-index views before compiling their upload bytes.</summary>
    private static CeresFlightMapDocument ReadMap(Stream json)
    {
        CeresFlightMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightMapDocument>(json,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres flight Mode-7 map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight Mode-7 map JSON.", error);
        }
        if (document.Version != CeresFlightArtworkFormat.Version ||
            document.Width != CeresFlightArtworkFormat.MapWidth ||
            document.Height != CeresFlightArtworkFormat.MapHeight ||
            !ValidView(document.FrontTiles) || !ValidView(document.RearTiles))
            throw new InvalidDataException(
                "Ceres flight map requires two ordered 32x24 arrays of eight-bit tile indexes.");
        return document;

        static bool ValidView(int[]? tiles) =>
            tiles is { Length: CeresFlightArtworkFormat.MapCellsPerView } &&
            tiles.All(tile => (uint)tile <= byte.MaxValue);
    }
}
