using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable character pixels, paired Mode-7 maps, and actor compositions for
/// the approach to Ceres.
/// Flight motion, DMA order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public sealed class CeresFlightArtworkCatalog
{
    /// <summary>Retains compiled DMA bytes and selected palette, OAM, and actor resources.</summary>
    private CeresFlightArtworkCatalog(byte[] floorCharacters, byte[] floorMaps,
        byte[] objectCharacters, CeresFlightPalette palette,
        CeresFlightSpritePresentation sprites, CeresFlightActorLayout actors)
    {
        FloorCharacters = floorCharacters;
        FloorMaps = floorMaps;
        ObjectCharacters = objectCharacters;
        Palette = palette;
        Sprites = sprites;
        Actors = actors;
    }

    /// <summary>Owned $4000-byte stream of 256 chunky 8-bpp Mode-7 characters, imported from <c>Tiles_Gunship_Ceres_Mode7</c> at <c>$95:A82F</c>; also reused by the ending flyaway.</summary>
    public ReadOnlyMemory<byte> FloorCharacters { get; }
    /// <summary>Front 768 map bytes followed by rear 768 map bytes.</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }
    /// <summary>Owned $4000-byte stream of 512 planar 4-bpp characters, imported from <c>Tiles_Space_Ceres</c> at <c>$96:D10A</c> and uploaded to VRAM bytes $C000-$FFFF.</summary>
    public ReadOnlyMemory<byte> ObjectCharacters { get; }

    /// <summary>The flight OBJ page the space, gunship and station compositions draw from: the OBJ sheet as $BCCD uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.CeresFlightObjects),
        ObjectCharacters.Span[..CeresFlightRomData.Transfers.ObjectCharacterByteCount]);
    /// <summary>Selected full 256-color RGB5 palette image for the approach; the native source is <c>$8C:E5E9</c>, separate from the PNGs' transport palettes.</summary>
    public CeresFlightPalette Palette { get; }
    /// <summary>Selected six-frame OAM compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction presentation.</summary>
    public CeresFlightSpritePresentation Sprites { get; }
    /// <summary>Selected initial screen-pixel positions for the five named rear-view actors; their movement and instruction timing remain compiled logic.</summary>
    public CeresFlightActorLayout Actors { get; }

    /// <summary>Identity of all selected approach maps, characters, colors, OAM frames and placements.</summary>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM transfer image of the Ceres flight palette")]
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresFlightArtworkCatalog), content =>
    {
        content.Append("mode7-characters", FloorCharacters.Span);
        content.Append("mode7-maps", FloorMaps.Span);
        content.Append("object-characters", ObjectCharacters.Span);
        Span<byte> paletteBytes = stackalloc byte[SuperMetroid.Core.Hardware.CgramPalettes.ByteCount];
        Palette.CopyTransferTo(paletteBytes);
        content.Append("palette", paletteBytes);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("actors", Convert.FromHexString(Actors.ContentIdentity));
    });

    /// <summary>Compiles the six selected Ceres approach resources into native character, map, palette, and actor presentation data.</summary>
    /// <param name="floorPng">128-by-128 indexed PNG with 256 palette entries, supplying 256 chunky 8-bpp characters.</param>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="objectPng">256-by-128 indexed PNG with sixteen palette entries, supplying 512 planar 4-bpp characters.</param>
    /// <param name="paletteJson">Full approach palette document with 256 RGB5 colors.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <returns>A catalog owning newly compiled transfer bytes and loaded presentation resources.</returns>
    /// <remarks>Reads from the streams' current positions without disposing them. PNG pixel indices define characters; the separate palette resource defines rendered colors. File provenance and stock hashes are validated by installation loading, not by this method.</remarks>
    /// <exception cref="ArgumentNullException">Any resource stream is null.</exception>
    /// <exception cref="InvalidDataException">A resource has an invalid format, dimensions, palette size, schema, map index, actor identity, composition, or compiled transfer length.</exception>
    public static CeresFlightArtworkCatalog Load(Stream floorPng, Stream mapJson,
        Stream objectPng, Stream paletteJson, Stream spritesJson, Stream actorsJson)
    {
        Ensure.NotNull(floorPng);
        Ensure.NotNull(mapJson);
        Ensure.NotNull(objectPng);
        Ensure.NotNull(paletteJson);
        Ensure.NotNull(spritesJson);
        Ensure.NotNull(actorsJson);
        IndexedPngImage mode7 = IndexedPng.Read(floorPng,
            new(CeresFlightArtworkFormat.FloorWidth, CeresFlightArtworkFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            new(CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight));
        if (mode7.Palette.Length != 256 || objects.Palette.Length != 16)
            throw new InvalidDataException("Ceres flight requires 256-color Mode-7 and 16-color OBJ PNGs.");

        CeresFlightMapDocument document = ReadMap(mapJson);
        byte[] map = new byte[2 * CeresFlightArtworkFormat.MapCellsPerView];
        for (int index = 0; index < CeresFlightArtworkFormat.MapCellsPerView; index++)
        {
            map[index] = checked((byte)document.FrontTiles[index]);
            map[CeresFlightArtworkFormat.MapCellsPerView + index] =
                checked((byte)document.RearTiles[index]);
        }
        byte[] characters = SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Size);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Size, 4);
        if (characters.Length != CeresFlightArtworkFormat.FloorByteCount ||
            objectCharacters.Length != CeresFlightArtworkFormat.ObjectByteCount)
            throw new InvalidDataException("Ceres flight PNGs compile to unexpected DMA lengths.");
        return new CeresFlightArtworkCatalog(characters, map, objectCharacters,
            CeresFlightPalette.Load(paletteJson),
            CeresFlightSpritePresentation.Load(spritesJson),
            CeresFlightActorLayout.Load(actorsJson));
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
