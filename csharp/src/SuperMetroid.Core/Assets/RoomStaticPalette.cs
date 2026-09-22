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

    public static RoomStaticPalette Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        RoomStaticPaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomStaticPaletteDocument>(json, JsonOptions)
                ?? throw new InvalidDataException("Room palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room palette JSON.", error);
        }
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
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)),
                (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
        }
        return new RoomStaticPalette(native);
    }

    public static void Write(Stream json, RoomStaticPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

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
    };
}

public sealed record RoomStaticPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>One editable RGB5 palette file per distinct graphics-set color source.</summary>
public static class RoomStaticPaletteFormat
{
    public const int Version = 1;
    public const int ColorCount = RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount / sizeof(ushort);

    public static string SourceFileName(int sourceAddress) => $"room-palette-{sourceAddress:X6}.json";
}
