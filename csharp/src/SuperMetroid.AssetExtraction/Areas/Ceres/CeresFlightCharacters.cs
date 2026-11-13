using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The approach to Ceres as the import pipeline reads it (#1409, #1636): its Mode 7 floor characters as chunky 8-bit pixels
/// and its OBJ characters as SNES 4-bpp planar bytes, its Mode 7 floor maps (#1693) and its compositions, whose parts are
/// the original's packed sprite placements (#1638), from the opening's import data, which the flight sheets compose from,
/// beside the installed documents. Its colours are a palette document only the import reads
/// (<see cref="ImportIntroCinematicArtwork.FlightPalette"/>); the game keeps their backdrop, which the opening's backdrop
/// document names (#1544).
/// </summary>
public sealed class CeresFlightCharacters : CeresFlightSourceArtwork
{
    /// <summary>Retains compiled characters and maps with the installed documents they are drawn by.</summary>
    /// <param name="floorCharacters">The chunky Mode 7 floor characters.</param>
    /// <param name="objectCharacters">The planar 4-bpp OBJ characters.</param>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="sprites">The approach's compositions.</param>
    /// <param name="documents">The installed backdrop and actor placements.</param>
    private CeresFlightCharacters(byte[] floorCharacters, byte[] objectCharacters, byte[] floorMaps,
        CeresFlightSpritePresentation sprites, CeresFlightSourceArtwork documents)
        : base(documents.Flight.Backdrop, documents.Flight.Actors)
    {
        FloorCharacters = floorCharacters;
        ObjectCharacters = objectCharacters;
        FloorMaps = floorMaps;
        Sprites = sprites;
    }

    /// <summary>Selected six-frame compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction scene's sheets.</summary>
    public CeresFlightSpritePresentation Sprites { get; }

    /// <summary>Front 768 map bytes followed by rear 768 map bytes, which the import pictures into the flight's floors (#1467).</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }

    /// <summary>Identity of the installed documents and of the maps and compositions the flight sheets are composed from.</summary>
    public override string ContentIdentity => SelectedPresentationHash.Create("CeresFlightImportArtwork-v2", content =>
    {
        content.Append("documents", Convert.FromHexString(base.ContentIdentity));
        content.Append("mode7-maps", FloorMaps.Span);
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
    });


    /// <summary>Owned $4000-byte stream of 256 chunky 8-bpp Mode-7 characters, imported from <c>Tiles_Gunship_Ceres_Mode7</c> at <c>$95:A82F</c>; also reused by the ending flyaway.</summary>
    public ReadOnlyMemory<byte> FloorCharacters { get; }

    /// <summary>Owned $4000-byte stream of 512 planar 4-bpp characters, imported from <c>Tiles_Space_Ceres</c> at <c>$96:D10A</c> and uploaded to VRAM bytes $C000-$FFFF.</summary>
    public ReadOnlyMemory<byte> ObjectCharacters { get; }

    /// <summary>The flight OBJ page the space, gunship and station compositions draw from: the OBJ sheet as $BCCD uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.CeresFlightObjects),
        ObjectCharacters.Span[..CeresFlightArtworkFileFormat.ObjectByteCount]);

    /// <summary>Compiles the Ceres approach's character pages and maps into native characters bound to its compositions and installed documents.</summary>
    /// <param name="floorPng">128-by-128 indexed page with 256 palette entries, supplying 256 chunky 8-bpp characters.</param>
    /// <param name="objectPng">256-by-128 indexed page with sixteen palette entries, supplying 512 planar 4-bpp characters.</param>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <param name="spritesJson">Document containing the six required named OAM compositions.</param>
    /// <param name="documents">The installed backdrop and actor placements the characters are drawn by.</param>
    /// <returns>A catalog owning newly compiled transfer bytes and the installed presentation resources.</returns>
    /// <remarks>Reads from the streams' current positions without disposing them. Page pixel indices define characters; the separate palette resource defines rendered colors.</remarks>
    /// <exception cref="ArgumentNullException">Any argument is null.</exception>
    /// <exception cref="InvalidDataException">A page has an invalid format, dimensions, palette size or compiled transfer length, or the map document an invalid schema or map index.</exception>
    public static CeresFlightCharacters Read(Stream floorPng, Stream objectPng, Stream mapJson, Stream spritesJson,
        CeresFlightSourceArtwork documents)
    {
        Ensure.NotNull(floorPng);
        Ensure.NotNull(objectPng);
        Ensure.NotNull(mapJson);
        Ensure.NotNull(spritesJson);
        Ensure.NotNull(documents);
        IndexedPngImage floorImage = IndexedPng.Read(floorPng,
            new(CeresFlightArtworkFileFormat.FloorWidth, CeresFlightArtworkFileFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(objectPng,
            new(CeresFlightArtworkFileFormat.ObjectWidth, CeresFlightArtworkFileFormat.ObjectHeight));
        if (floorImage.Colours.Length != 256 || objects.Colours.Length != 16)
            throw new InvalidDataException("Ceres flight requires 256-color Mode-7 and 16-color OBJ PNGs.");

        byte[] characters = SnesMode7TileEncoder.Encode(floorImage.Pixels, floorImage.Size);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels,
            objects.Size, 4);
        if (characters.Length != CeresFlightArtworkFileFormat.FloorByteCount ||
            objectCharacters.Length != CeresFlightArtworkFileFormat.ObjectByteCount)
            throw new InvalidDataException("Ceres flight PNGs compile to unexpected DMA lengths.");
        return new CeresFlightCharacters(characters, objectCharacters, ReadMaps(mapJson),
            CeresFlightSpritePresentation.Load(spritesJson), documents);
    }


    /// <summary>Reads both ordered 768-byte front and rear map slices from a version-one map document.</summary>
    /// <param name="mapJson">Version-one document containing both ordered 768-byte front/rear map slices.</param>
    /// <returns>Front 768 map bytes followed by rear 768 map bytes.</returns>
    /// <exception cref="InvalidDataException">The document has an unsupported version, dimensions, array length, or tile index.</exception>
    private static byte[] ReadMaps(Stream mapJson)
    {
        CeresFlightMapDocument document = ReadMap(Ensure.NotNull(mapJson));
        byte[] map = new byte[2 * CeresFlightArtworkFileFormat.MapCellsPerView];
        for (int index = 0; index < CeresFlightArtworkFileFormat.MapCellsPerView; index++)
        {
            map[index] = checked((byte)document.FrontViewTiles[index]);
            map[CeresFlightArtworkFileFormat.MapCellsPerView + index] =
                checked((byte)document.RearViewTiles[index]);
        }
        return map;
    }

    /// <summary>Serializes a front/rear map document to the destination.</summary>
    /// <param name="json">Caller-owned writable destination stream, written at its current position.</param>
    /// <param name="document">Version-one document with two complete 32-by-24 editing arrays containing tile indices $00-$FF.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    public static void WriteMap(Stream json, CeresFlightMapDocument document)
    {
        Ensure.NotNull(json);
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
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
        if (document.Version != CeresFlightArtworkFileFormat.Version ||
            document.Width != CeresFlightArtworkFileFormat.MapWidth ||
            document.Height != CeresFlightArtworkFileFormat.MapHeight ||
            !ValidView(document.FrontViewTiles) || !ValidView(document.RearViewTiles))
            throw new InvalidDataException(
                "Ceres flight map requires two ordered 32x24 arrays of eight-bit tile indexes.");
        return document;

        static bool ValidView(int[]? tiles) =>
            tiles is { Length: CeresFlightArtworkFileFormat.MapCellsPerView } &&
            tiles.All(tile => (uint)tile <= byte.MaxValue);
    }
}
