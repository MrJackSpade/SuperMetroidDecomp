using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable scanline color-math presentation for the sixteen title zoom buckets.
/// The selected lines contain no HDMA addresses, instruction streams, or gameplay state.
/// </summary>
public sealed class TitleGradientPresentation
{
    private readonly TitleGradientLine[][] variants;

    private TitleGradientPresentation(TitleGradientLine[][] variants) => this.variants = variants;

    /// <summary>Returns the authored scanlines selected by native zoom bits four through seven.</summary>
    /// <param name="zoom">Native Mode-7 zoom word; only bits 4..7 select a variant, with all other bits ignored.</param>
    /// <returns>A borrowed read-only view of the selected presentation's 224 compiled scanlines in top-to-bottom screen order.</returns>
    public ReadOnlySpan<TitleGradientLine> Resolve(ushort zoom) =>
        variants[(zoom & 0xf0) >> 4];

    /// <summary>Validates all sixteen authored zoom variants and compiles their independent per-scanline fixed colors and color-math controls.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open for the caller.</param>
    /// <returns>An immutable presentation whose scanlines do not retain the document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or variant ordering/count is wrong, or a variant lacks 224 valid RGB5/control-byte scanlines.</exception>
    public static TitleGradientPresentation Load(Stream json)
    {
        TitleGradientDocument document;
        try
        {
            document = JsonAssetDocument.Read<TitleGradientDocument>(
                json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Title gradient presentation is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title gradient presentation JSON.", error);
        }

        if (document.Version != TitleGradientFormat.Version ||
            document.Variants is null ||
            document.Variants.Length != TitleGradientFormat.VariantCount)
        {
            throw new InvalidDataException(
                $"Title gradient requires version {TitleGradientFormat.Version} and " +
                $"{TitleGradientFormat.VariantCount} zoom variants.");
        }

        var variants = new TitleGradientLine[TitleGradientFormat.VariantCount][];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            TitleGradientVariant? source = document.Variants[variant];
            if (source is null || source.ZoomHighNibble != variant || source.Lines is null ||
                source.Lines.Length != SnesPpuLayout.ScreenHeightPixels)
            {
                throw new InvalidDataException(
                    $"Title gradient variant {variant} must identify nibble {variant} and contain " +
                    $"{SnesPpuLayout.ScreenHeightPixels} scanlines.");
            }

            variants[variant] = new TitleGradientLine[source.Lines.Length];
            for (int line = 0; line < source.Lines.Length; line++)
            {
                TitleGradientScanline? value = source.Lines[line];
                if (value is null || value.Red is < 0 or > 31 || value.Green is < 0 or > 31 ||
                    value.Blue is < 0 or > 31 || value.ColorMathControl is < 0 or > byte.MaxValue)
                {
                    throw new InvalidDataException(
                        $"Title gradient variant {variant}, scanline {line} requires RGB5 colors " +
                        "and an eight-bit color-math control value.");
                }
                variants[variant][line] = new(
                    (byte)value.Red,
                    (byte)value.Green,
                    (byte)value.Blue,
                    (byte)value.ColorMathControl);
            }
        }
        return new TitleGradientPresentation(variants);
    }

    /// <summary>Serializes and validates every gradient variant before writing any UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Authored gradient whose mutable arrays are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or scanline-value validation.</exception>
    public static void Write(Stream json, TitleGradientDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable expanded title-gradient schema; native HDMA runs are represented by sixteen complete screen-height color/control arrays.</summary>
public sealed record TitleGradientDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitleGradientFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull variants in zoom-nibble order 0 through 15; this array and its nested line arrays remain caller-mutable until compilation.</summary>
    public required TitleGradientVariant[] Variants { get; init; }
}

/// <summary>One title zoom bucket corresponding to a pointer in native TitleSequenceHDMATables at $8C:BC5D, expanded into screen scanlines.</summary>
public sealed record TitleGradientVariant
{
    /// <summary>Zero through fifteen; native selects this with zoom bits four through seven.</summary>
    public required int ZoomHighNibble { get; init; }
    /// <summary>Exactly 224 nonnull scanlines indexed by visible screen Y from 0 through 223, not HDMA commands or run lengths.</summary>
    public required TitleGradientScanline[] Lines { get; init; }
}

/// <summary>One scanline's effective fixed RGB5 color after native COLDATA writes, plus the CGADSUB byte governing its color arithmetic.</summary>
public sealed record TitleGradientScanline
{
    /// <summary>Fixed-color red intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Red { get; init; }
    /// <summary>Fixed-color green intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Green { get; init; }
    /// <summary>Fixed-color blue intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Blue { get; init; }
    /// <summary>Raw CGADSUB value from 0 through 255. The title renderer uses bit 0 for BG1/backdrop, bit 4 for OBJ palettes 4..7 and bit 7 for subtraction; native $88:EB95 supplies $A1/$31 runs.</summary>
    public required int ColorMathControl { get; init; }
}

/// <summary>Published filename and schema dimensions for the expanded title fixed-color gradient, independently editable without native HDMA pointers.</summary>
public static class TitleGradientFormat
{
    /// <summary>Installed JSON filename selected by the title presentation catalog.</summary>
    public const string FileName = "title-gradient.json";
    /// <summary>Revision 1 of the sixteen-variant, per-scanline RGB5/control-byte schema.</summary>
    public const int Version = 1;
    /// <summary>Sixteen variants covering every value of the native zoom word's bits 4 through 7.</summary>
    public const int VariantCount = 16;
}
