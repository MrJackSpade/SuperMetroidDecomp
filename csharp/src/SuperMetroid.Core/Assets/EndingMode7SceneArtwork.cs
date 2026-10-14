using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One editable ending backdrop. The low-byte map is copied into both native
/// $2000-word halves; the high-byte character lane supplies the Mode-7 pixels.
/// The discarded odd bytes of the native packed map source never reach visible
/// VRAM because the subsequent high-byte-only transfer overwrites them.
/// </summary>
public sealed class EndingMode7SceneArtwork
{
    /// <summary>Retains the validated low-byte map and high-byte character lanes for one scene.</summary>
    private EndingMode7SceneArtwork(byte[] map, byte[] characters)
    {
        Map = map;
        Characters = characters;
    }

    /// <summary>The $2000 row-ordered tile indices written to the low VRAM byte lane at words $0000 and $2000.</summary>
    public ReadOnlyMemory<byte> Map { get; }
    /// <summary>The $4000 chunky pixel bytes for 256 ordered 8x8 tiles, written to the high VRAM byte lane.</summary>
    public ReadOnlyMemory<byte> Characters { get; }

    /// <summary>Validates a 128x64 map and a 128x128 indexed character atlas, then compiles their separate native Mode-7 lanes.</summary>
    /// <param name="mapJson">Version-1 JSON containing 8192 ordered tile indices in the range 0..255.</param>
    /// <param name="charactersPng">A 256-color indexed PNG whose 16x16 grid supplies the 256 character tiles; palette colors are not imported here.</param>
    /// <returns>The decoded map lane and tile-ordered character lane.</returns>
    /// <exception cref="ArgumentNullException">Either input stream is null.</exception>
    /// <exception cref="InvalidDataException">The map schema, atlas format, palette length, or native transfer length is invalid.</exception>
    public static EndingMode7SceneArtwork Load(Stream mapJson, Stream charactersPng)
    {
        Ensure.NotNull(mapJson);
        Ensure.NotNull(charactersPng);
        EndingMode7MapDocument document = ReadMap(mapJson);
        IndexedPngImage image = IndexedPng.Read(charactersPng,
            new(EndingMode7ArtworkFormat.CharacterWidth, EndingMode7ArtworkFormat.CharacterHeight));
        if (image.Palette.Length != 256)
            throw new InvalidDataException("Ending Mode-7 characters require a 256-color indexed PNG.");
        byte[] characters = SnesMode7TileEncoder.Encode(image.Pixels,
            image.Size);
        if (characters.Length != EndingMode7ArtworkFormat.CharacterByteCount)
            throw new InvalidDataException("Ending Mode-7 PNG has the wrong native transfer length.");
        return new EndingMode7SceneArtwork(
            document.Tiles.Select(tile => checked((byte)tile)).ToArray(), characters);
    }

    /// <summary>Serializes an ending backdrop map, validating the complete encoded document before writing any bytes to the destination.</summary>
    /// <param name="json">Destination stream for the map JSON.</param>
    /// <param name="document">Version-1, 128x64 map with exactly 8192 eight-bit tile indices.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the ending backdrop map schema.</exception>
    public static void WriteMap(Stream json, EndingMode7MapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = ReadMap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Loads and validates the 128-by-64 backdrop half-map and its byte-valued tile indices.</summary>
    private static EndingMode7MapDocument ReadMap(Stream json)
    {
        EndingMode7MapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<EndingMode7MapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ending Mode-7 map JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending Mode-7 map JSON.", error);
        }
        if (document.Version != EndingMode7ArtworkFormat.Version ||
            document.Width != EndingMode7ArtworkFormat.MapWidth ||
            document.Height != EndingMode7ArtworkFormat.MapHeight ||
            document.Tiles is not { Length: EndingMode7ArtworkFormat.MapByteCount } ||
            document.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException(
                "Ending Mode-7 map requires 128x64 ordered eight-bit tile indexes.");
        return document;
    }
}
