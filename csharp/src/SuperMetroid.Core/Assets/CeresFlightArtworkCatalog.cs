using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable character pixels, paired Mode-7 maps, and actor compositions for
/// the approach to Ceres.
/// Flight motion, DMA order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public sealed class CeresFlightArtworkCatalog
{
    /// <summary>Stores the compiled transfer buffers and selected presentation resources as one flight catalog.</summary>
    /// <param name="mode7Characters">Encoded chunky Mode-7 character bytes owned by the catalog.</param>
    /// <param name="mode7Maps">Ordered front and rear Mode-7 map upload bytes.</param>
    /// <param name="objectCharacters">Encoded planar OBJ character bytes owned by the catalog.</param>
    /// <param name="palette">Selected full approach palette.</param>
    /// <param name="sprites">Selected actor OAM compositions.</param>
    /// <param name="actors">Selected initial actor placements.</param>
    private CeresFlightArtworkCatalog(byte[] mode7Characters, byte[] mode7Maps,
        byte[] objectCharacters, CeresFlightPalette palette,
        CeresFlightSpritePresentation sprites, CeresFlightActorLayout actors)
    {
        Mode7Characters = mode7Characters;
        Mode7Maps = mode7Maps;
        ObjectCharacters = objectCharacters;
        Palette = palette;
        Sprites = sprites;
        Actors = actors;
    }

    /// <summary>Owned $4000-byte stream of 256 chunky 8-bpp Mode-7 characters, imported from <c>Tiles_Gunship_Ceres_Mode7</c> at <c>$95:A82F</c>; also reused by the ending flyaway.</summary>
    public ReadOnlyMemory<byte> Mode7Characters { get; }
    /// <summary>Front 768 map bytes followed by rear 768 map bytes.</summary>
    public ReadOnlyMemory<byte> Mode7Maps { get; }
    /// <summary>Owned $4000-byte stream of 512 planar 4-bpp characters, imported from <c>Tiles_Space_Ceres</c> at <c>$96:D10A</c> and uploaded to VRAM bytes $C000-$FFFF.</summary>
    public ReadOnlyMemory<byte> ObjectCharacters { get; }
    /// <summary>Selected full 256-color RGB5 palette image for the approach; the native source is <c>$8C:E5E9</c>, separate from the PNGs' transport palettes.</summary>
    public CeresFlightPalette Palette { get; }
    /// <summary>Selected six-frame OAM compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction presentation.</summary>
    public CeresFlightSpritePresentation Sprites { get; }
    /// <summary>Selected initial screen-pixel positions for the five named rear-view actors; their movement and instruction timing remain compiled logic.</summary>
    public CeresFlightActorLayout Actors { get; }

    /// <summary>Identity of all selected approach maps, characters, colors, OAM frames and placements.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresFlightArtworkCatalog), content =>
    {
        content.Append("mode7-characters", Mode7Characters.Span);
        content.Append("mode7-maps", Mode7Maps.Span);
        content.Append("object-characters", ObjectCharacters.Span);
        Span<byte> paletteBytes = stackalloc byte[SuperMetroid.Core.Hardware.SnesCgram.ByteCount];
        Palette.CopyTransferTo(paletteBytes);
        content.Append("palette", paletteBytes);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("actors", Convert.FromHexString(Actors.ContentIdentity));
    });

    /// <summary>Compiles the six selected Ceres approach resources into native character, map, palette, and actor presentation data.</summary>
    /// <param name="mode7Png">128-by-128 indexed PNG with 256 palette entries, supplying 256 chunky 8-bpp characters.</param>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="objectPng">256-by-128 indexed PNG with sixteen palette entries, supplying 512 planar 4-bpp characters.</param>
    /// <param name="paletteJson">Full approach palette document with 256 RGB5 colors.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="actorsJson">Document containing the five required ordered rear-view actor identities and initial coordinates.</param>
    /// <returns>A catalog owning newly compiled transfer bytes and loaded presentation resources.</returns>
    /// <remarks>Reads from the streams' current positions without disposing them. PNG pixel indices define characters; the separate palette resource defines rendered colors. File provenance and stock hashes are validated by installation loading, not by this method.</remarks>
    /// <exception cref="ArgumentNullException">Any resource stream is null.</exception>
    /// <exception cref="InvalidDataException">A resource has an invalid format, dimensions, palette size, schema, map index, actor identity, composition, or compiled transfer length.</exception>
    public static CeresFlightArtworkCatalog Load(Stream mode7Png, Stream mapJson,
        Stream objectPng, Stream paletteJson, Stream spritesJson, Stream actorsJson)
    {
        ArgumentNullException.ThrowIfNull(mode7Png);
        ArgumentNullException.ThrowIfNull(mapJson);
        ArgumentNullException.ThrowIfNull(objectPng);
        ArgumentNullException.ThrowIfNull(paletteJson);
        ArgumentNullException.ThrowIfNull(spritesJson);
        ArgumentNullException.ThrowIfNull(actorsJson);
        IndexedPngImage mode7 = IndexedPng.Read(mode7Png,
            CeresFlightArtworkFormat.Mode7Width, CeresFlightArtworkFormat.Mode7Height);
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight);
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
        byte[] characters = SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Width, mode7.Height);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Width, objects.Height, 4);
        if (characters.Length != CeresFlightArtworkFormat.Mode7ByteCount ||
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
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Loads and validates both ordered tile-index arrays from a Ceres flight map document.</summary>
    /// <param name="json">Readable stream positioned at the start of the map document.</param>
    /// <returns>The validated front and rear map arrays in their editing representation.</returns>
    /// <exception cref="InvalidDataException">The JSON is malformed or its schema, dimensions, array lengths, or tile indices are invalid.</exception>
    private static CeresFlightMapDocument ReadMap(Stream json)
    {
        CeresFlightMapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightMapDocument>(json,
                MapPresentationFormat.JsonOptions)
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

/// <summary>Editable front/rear map slices from <c>Gunship_Ceres_Tilemap</c> at <c>$96:FE69</c>, preserving native transfer order.</summary>
/// <remarks>The 32-by-24 dimensions describe the host editing arrays, not a replacement for the hardware's full Mode-7 map geometry. Arrays are caller-owned mutable document data and are copied into byte transfers when loaded.</remarks>
public sealed record CeresFlightMapDocument
{
    /// <summary>Schema revision; loading requires <see cref="CeresFlightArtworkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Host editing-array width in tile cells; must be 32 for each view.</summary>
    public required int Width { get; init; }
    /// <summary>Host editing-array height in tile cells; must be 24 for each view.</summary>
    public required int Height { get; init; }
    /// <summary>Front-view tile indices $00-$FF in ordered native upload-byte sequence; exactly 768 entries, preceding the rear slice.</summary>
    public required int[] FrontTiles { get; init; }
    /// <summary>Rear-view tile indices $00-$FF in ordered native upload-byte sequence; exactly 768 entries, selected after the initial camera approach.</summary>
    public required int[] RearTiles { get; init; }
}

/// <summary>File identities and exact Ceres approach artwork geometry.</summary>
public static class CeresFlightArtworkFormat
{
    /// <summary>Required schema revision of the paired front/rear map document.</summary>
    public const int Version = 1;
    /// <summary>Indexed PNG filename for the shared gunship/Ceres chunky Mode-7 characters.</summary>
    public const string Mode7FileName = "ceres-flight-mode7-characters.png";
    /// <summary>JSON filename for the two ordered approach map slices, distinct from the later destruction map slice.</summary>
    public const string MapFileName = "ceres-flight-mode7-maps.json";
    /// <summary>Indexed PNG filename for the space/Ceres planar OBJ characters and reused SPACE COLONY background characters.</summary>
    public const string ObjectFileName = "ceres-flight-object-characters.png";
    /// <summary>Mode-7 PNG width in pixels: sixteen 8-pixel character columns.</summary>
    public const int Mode7Width = 128;
    /// <summary>Mode-7 PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int Mode7Height = 128;
    /// <summary>Exact chunky Mode-7 character length, $4000 bytes for 256 characters at 64 bytes each.</summary>
    public const int Mode7ByteCount = 0x4000;
    /// <summary>OBJ PNG width in pixels: thirty-two 8-pixel character columns.</summary>
    public const int ObjectWidth = 256;
    /// <summary>OBJ PNG height in pixels: sixteen 8-pixel character rows.</summary>
    public const int ObjectHeight = 128;
    /// <summary>Exact planar 4-bpp character length, $4000 bytes for 512 characters at 32 bytes each.</summary>
    public const int ObjectByteCount = 0x4000;
    /// <summary>Number of tile cells per row in each view's host editing-array layout, not the full hardware Mode-7 map stride.</summary>
    public const int MapWidth = 32;
    /// <summary>Number of tile-cell rows in each view's host editing-array layout.</summary>
    public const int MapHeight = 24;
    /// <summary>Number of one-byte tile indices in each front or rear upload slice: 768 ($0300).</summary>
    public const int MapCellsPerView = MapWidth * MapHeight;
}
