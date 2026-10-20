using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>One exact BGR555 color image, independently editable as RGB5 JSON.</summary>
public sealed class EndingPalette
{
    /// <summary>Exact selected BGR555 bytes for a static palette or independently edited logo fade.</summary>
    private readonly byte[]? nativeBytes;
    /// <summary>Endpoint-backed logo fade used only when it reproduces every supplied crossfade color.</summary>
    private readonly EndingLogoPaletteFade? logoFade;

    /// <summary>Retains a validated native color image without recalculating its supplied inks.</summary>
    private EndingPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;
    /// <summary>Retains the verified logo interpolation instead of a duplicate 512-color table.</summary>
    private EndingPalette(EndingLogoPaletteFade logoFade) => this.logoFade = logoFade;

    /// <summary>Transfer/export view. A computed fade is materialized only for this
    /// request, never stored as a generated lookup cache; gameplay calls Color directly.</summary>
    public ReadOnlyMemory<byte> Transfer
    {
        get
        {
            if (nativeBytes is not null) return nativeBytes;
            var bytes = new byte[ColorCount * Bgr555.ByteCount];
            WriteColors(bytes, 0, ColorCount);
            return bytes;
        }
    }
    /// <summary>Number of color words in the complete resource, including all 512 ordered colors when this is the sixteen-step logo crossfade.</summary>
    public int ColorCount => nativeBytes is not null ? nativeBytes.Length / Bgr555.ByteCount : EndingLogoPaletteFade.ColorCount;

    /// <summary>Reads one selected color, evaluating a recognized logo fade directly rather than materializing its transfer image.</summary>
    /// <param name="index">Zero-based color position within <see cref="ColorCount"/>; crossfade positions use (step * 2 + palette) * 16 + color, with BG palette 0 and OBJ palette 1.</param>
    /// <returns>A native BGR555 word: red in bits 0..4, green in 5..9 and blue in 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this resource.</exception>
    public Bgr555 Color(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (logoFade is not null) return logoFade.Color(index);
        return Bgr555.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(
            nativeBytes.AsSpan(index * Bgr555.ByteCount)));
    }

    /// <summary>Preserves the cartridge's partial source/destination CGRAM transfers.</summary>
    /// <param name="cgram">Destination color memory; colors outside the requested transfer are left unchanged.</param>
    /// <param name="sourceColor">Zero-based starting color in this resource, not a byte offset.</param>
    /// <param name="count">Number of consecutive color words to transfer; zero is allowed.</param>
    /// <param name="destinationColor">Zero-based starting CGRAM color index; the complete transfer must fit its 256 colors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The source range or destination range is outside its color memory.</exception>
    [PpuLayoutBoundary("codec: native ending palette transfers at their CGRAM index operands")]
    public void LoadTo(IPaletteColors cgram, int sourceColor, int count, int destinationColor)
    {
        Ensure.NotNull(cgram);
        if (sourceColor < 0 || count < 0 || sourceColor > ColorCount - count)
            throw new ArgumentOutOfRangeException(nameof(sourceColor));
        if (nativeBytes is not null)
            new CgramPalettes(cgram).All.LoadPortBytes(nativeBytes.AsSpan(sourceColor * Bgr555.ByteCount,
                count * Bgr555.ByteCount), destinationColor);
        else
        {
            Span<byte> transfer = stackalloc byte[count * Bgr555.ByteCount];
            WriteColors(transfer, sourceColor, count);
            new CgramPalettes(cgram).All.LoadPortBytes(transfer, destinationColor);
        }
    }

    /// <summary>Materializes a requested contiguous color range as little-endian BGR555 words.</summary>
    private void WriteColors(Span<byte> bytes, int sourceColor, int count)
    {
        for (int i = 0; i < count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.Slice(i * Bgr555.ByteCount), Color(sourceColor + i).ToWord());
    }

    /// <summary>Validates RGB5 JSON against the chosen resource's exact color count and compiles an independent native color representation.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <param name="id">Palette role determining the required full resource size, not the size of an individual CGRAM transfer.</param>
    /// <returns>Selected colors copied from the document; a logo crossfade matching the interpolation rule is represented by its endpoints without changing any supplied colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The JSON is invalid or ambiguous, the version or color count is wrong, or a color is null or has a channel outside 0..31.</exception>
    public static EndingPalette Load(Stream json, EndingPaletteId id)
    {
        EndingPaletteDocument document = JsonAssetDocument.Read<EndingPaletteDocument>(
            json, ArtworkJson.Options, $"ending {id} palette");
        int count = EndingPaletteDefinitions.ColorCount(id);
        if (document.Version != EndingPaletteDefinitions.Version ||
            document.Colors is null || document.Colors.Length != count)
            throw new InvalidDataException($"Ending {id} palette requires {count} RGB5 colors.");
        var native = new byte[count * Bgr555.ByteCount];
        for (int index = 0; index < count; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ending {id} palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * Bgr555.ByteCount),
                color.ToBgr555().ToWord());
        }
        if (id == EndingPaletteId.LogoCrossfade && EndingLogoPaletteFade.TryCreate(native) is { } fade)
            return new EndingPalette(fade);
        return new EndingPalette(native);
    }

    /// <summary>Serializes and validates the entire selected palette before writing its UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="id">Palette role used to validate the document's color count.</param>
    /// <param name="document">Authored color collection read for serialization, not retained by the writer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or RGB5 validation.</exception>
    public static void Write(Stream json, EndingPaletteId id, EndingPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false), id);
        json.Write(bytes);
    }
}
