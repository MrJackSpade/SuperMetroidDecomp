using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>The 128 base BG colors selected by one room graphics-set palette source.</summary>
public sealed class RoomStaticPalette
{
    private readonly byte[] nativeBytes;

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
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Loads all 128 colors into CGRAM beginning at color zero.</summary>
    /// <param name="cgram">The color memory that receives the 256-byte native transfer.</param>
    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        cgram.LoadBytes(nativeBytes);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}

/// <summary>Editable JSON representation of one room graphics-set palette source.</summary>
public sealed record RoomStaticPaletteDocument
{
    /// <summary>Gets the room-palette schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the exactly 128 RGB5 colors in native CGRAM order.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>One editable RGB5 palette file per distinct graphics-set color source.</summary>
public static class RoomStaticPaletteFormat
{
    /// <summary>The supported room-palette JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The number of RGB5 colors in one room graphics-set palette source.</summary>
    public const int ColorCount = RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount / Bgr555.ByteCount;

    /// <summary>Builds the editable file name for a palette's 24-bit native source address.</summary>
    /// <param name="sourceAddress">The palette's 24-bit native source address.</param>
    /// <returns>A file name in the form <c>room-palette-XXXXXX.json</c>.</returns>
    public static string SourceFileName(int sourceAddress) => $"room-palette-{sourceAddress:X6}.json";
}
