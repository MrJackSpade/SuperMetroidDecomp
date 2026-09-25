using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The opening narration's full, editable native-precision CGRAM image.</summary>
public sealed class IntroCinematicPalette
{
    private readonly byte[] nativeBytes;

    private IntroCinematicPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;

    /// <summary>Exact BGR555 bytes copied by the cartridge's opening setup.</summary>
    public ReadOnlyMemory<byte> Transfer => nativeBytes;

    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        cgram.LoadBytes(nativeBytes);
    }

    public static IntroCinematicPalette Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroCinematicPaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<IntroCinematicPaletteDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Opening palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening palette JSON.", error);
        }
        if (document.Version != IntroCinematicPaletteFormat.Version ||
            document.Colors is not { Length: SnesCgram.ColorCount })
            throw new InvalidDataException("Opening palette requires 256 RGB5 colors.");

        var native = new byte[SnesCgram.ByteCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Opening palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)),
                (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
        }
        return new IntroCinematicPalette(native);
    }

    public static void Write(Stream json, IntroCinematicPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroCinematicPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    public const int Version = 1;
    public const string FileName = "intro-narration-palette.json";
}
