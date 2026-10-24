using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed approach to Ceres (#1409): its paired Mode 7 maps, palette, actor compositions and placements, and how
/// many Mode 7 floor and OBJ characters its artwork holds. The installed flight sheets picture the characters; their
/// pixels belong to the import pipeline that reads the installed artwork, which also names the content's identity.
/// Flight motion, DMA order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public abstract class CeresFlightArtworkCatalog
{
    /// <summary>Retains the selected maps, palette, OAM and actor resources and the installed characters' counts.</summary>
    /// <param name="floorMaps">Front 768 map bytes followed by rear 768 map bytes.</param>
    /// <param name="palette">The approach palette.</param>
    /// <param name="sprites">The approach's OAM compositions.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    /// <param name="floorCharacterCount">The installed Mode 7 floor characters.</param>
    /// <param name="objectCharacterCount">The installed OBJ characters.</param>
    internal CeresFlightArtworkCatalog(byte[] floorMaps, CeresFlightPalette palette,
        CeresFlightSpritePresentation sprites, CeresFlightActorLayout actors, int floorCharacterCount, int objectCharacterCount)
    {
        FloorMaps = floorMaps;
        Palette = palette;
        Sprites = sprites;
        Actors = actors;
        FloorCharacterCount = floorCharacterCount;
        ObjectCharacterCount = objectCharacterCount;
    }

    /// <summary>The installed Mode 7 floor characters, also reused by the ending flyaway.</summary>
    public int FloorCharacterCount { get; }
    /// <summary>Front 768 map bytes followed by rear 768 map bytes.</summary>
    public ReadOnlyMemory<byte> FloorMaps { get; }
    /// <summary>The installed space and Ceres OBJ characters.</summary>
    public int ObjectCharacterCount { get; }
    /// <summary>Selected full 256-color RGB5 palette image for the approach; the native source is <c>$8C:E5E9</c>, separate from the PNGs' transport palettes.</summary>
    public CeresFlightPalette Palette { get; }
    /// <summary>Selected six-frame OAM compositions for stars, station, asteroids, and vortex, shared with the Ceres destruction presentation.</summary>
    public CeresFlightSpritePresentation Sprites { get; }
    /// <summary>Selected initial screen-pixel positions for the five named rear-view actors; their movement and instruction timing remain compiled logic.</summary>
    public CeresFlightActorLayout Actors { get; }

    /// <summary>Identity of all selected approach maps, characters, colors, OAM frames and placements.</summary>
    public abstract string ContentIdentity { get; }

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
