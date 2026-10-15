using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The 128 base BG colors selected by one room graphics-set palette source.</summary>
public sealed class RoomStaticPalette
{
    /// <summary>Validated little-endian BGR555 transfer, owned independently of the JSON authoring data.</summary>
    private readonly byte[] nativeBytes;

    /// <summary>Wraps the validated CGRAM transfer assembled by <see cref="Load"/>.</summary>
    private RoomStaticPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;

    /// <summary>Exact native-size CGRAM transfer after discarding the unused high color bit.</summary>
    public ReadOnlyMemory<byte> Transfer => nativeBytes;

    /// <summary>
    /// Loads one room graphics-set palette from a JSON document containing exactly 128 RGB5 colors.
    /// </summary>
    /// <param name="json">The caller-owned stream containing the palette document.</param>
    /// <returns>The compiled 256-byte little-endian CGRAM transfer.</returns>
    /// <exception cref="InvalidDataException">
    /// The document version, color count, or an RGB5 component is invalid.
    /// </exception>
    public static RoomStaticPalette Load(Stream json)
    {
        RoomStaticPaletteDocument document = JsonAssetDocument.Read<RoomStaticPaletteDocument>(
            json, JsonOptions, "room palette");
        if (document.Version != RoomStaticPaletteFormat.Version ||
            document.Colors is null || document.Colors.Length != RoomStaticPaletteFormat.ColorCount)
            throw new InvalidDataException(
                $"Room palette requires version {RoomStaticPaletteFormat.Version} and " +
                $"{RoomStaticPaletteFormat.ColorCount} RGB5 colors.");

        var native = new byte[RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Room palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * Bgr555.ByteCount),
                color.ToBgr555().ToWord());
        }
        return new RoomStaticPalette(native);
    }

    /// <summary>Validates and writes one editable room-palette document as JSON.</summary>
    /// <param name="json">The caller-owned destination stream.</param>
    /// <param name="document">The palette document to validate and serialize.</param>
    public static void Write(Stream json, RoomStaticPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Loads all 128 colors into CGRAM beginning at color zero.</summary>
    /// <param name="cgram">The color memory that receives the 256-byte native transfer.</param>
    public void LoadTo(SnesCgram cgram)
    {
        Ensure.NotNull(cgram);
        new CgramPalettes(cgram).Background.LoadPortBytes(nativeBytes);
    }

    /// <summary>Uses camel-case JSON and rejects unknown properties so palette schema mistakes fail during loading.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}
