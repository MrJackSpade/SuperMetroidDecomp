using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The Ceres approach's full, editable 256-color CGRAM image.</summary>
public sealed class CeresFlightPalette
{
    private readonly byte[] nativeBytes;

    private CeresFlightPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;

    /// <summary>Exact BGR555 bytes transferred before the first flight frame.</summary>
    public ReadOnlyMemory<byte> Transfer => nativeBytes;

    public static CeresFlightPalette Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresFlightPaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresFlightPaletteDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres flight palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight palette JSON.", error);
        }
        if (document.Version != CeresFlightPaletteFormat.Version ||
            document.Colors is not { Length: SnesCgram.ColorCount })
            throw new InvalidDataException("Ceres flight palette requires 256 RGB5 colors.");

        var native = new byte[SnesCgram.ByteCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)),
                (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
        }
        return new CeresFlightPalette(native);
    }

    public static void Write(Stream json, CeresFlightPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        cgram.LoadBytes(nativeBytes);
    }
}

public sealed record CeresFlightPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the native Ceres approach palette.</summary>
public static class CeresFlightPaletteFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-flight-palette.json";
}
