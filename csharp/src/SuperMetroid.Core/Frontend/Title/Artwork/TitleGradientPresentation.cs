using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Editable scanline color-math presentation for the sixteen title zoom buckets.
/// The selected lines contain no HDMA addresses, instruction streams, or gameplay state.
/// </summary>
public sealed class TitleGradientPresentation
{
    /// <summary>Sixteen independently authored 224-line gradients indexed by the zoom high nibble.</summary>
    private readonly TitleGradientLine[][] variants;

    /// <summary>Retains validated, owned title gradient scanline arrays.</summary>
    private TitleGradientPresentation(TitleGradientLine[][] variants) => this.variants = variants;

    /// <summary>Returns the authored scanlines selected by native zoom bits four through seven.</summary>
    /// <param name="zoom">Native Mode-7 zoom word; only bits 4..7 select a variant, with all other bits ignored.</param>
    /// <returns>A borrowed read-only view of the selected presentation's 224 compiled scanlines in top-to-bottom screen order.</returns>
    public ReadOnlySpan<TitleGradientLine> Resolve(int zoom) =>
        variants[(zoom >> 4) % 16];

    /// <summary>Validates all sixteen authored zoom variants and compiles their independent per-scanline colours and blends.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open for the caller.</param>
    /// <returns>An immutable presentation whose scanlines do not retain the document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or variant ordering/count is wrong, or a variant lacks 224 scanlines with valid RGB5 colours.</exception>
    public static TitleGradientPresentation Load(Stream json)
    {
        TitleGradientDocument document;
        try
        {
            document = JsonAssetDocument.Read<TitleGradientDocument>(
                json,
                ArtworkJson.Options)
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
                source.Lines.Length != ScreenGeometry.ScreenHeightPixels)
            {
                throw new InvalidDataException(
                    $"Title gradient variant {variant} must identify nibble {variant} and contain " +
                    $"{ScreenGeometry.ScreenHeightPixels} scanlines.");
            }

            variants[variant] = new TitleGradientLine[source.Lines.Length];
            for (int line = 0; line < source.Lines.Length; line++)
            {
                TitleGradientScanline? value = source.Lines[line];
                if (value is null || value.Red is < 0 or > 31 || value.Green is < 0 or > 31 ||
                    value.Blue is < 0 or > 31)
                {
                    throw new InvalidDataException(
                        $"Title gradient variant {variant}, scanline {line} requires RGB5 colors.");
                }
                variants[variant][line] = new(value.Red, value.Green, value.Blue, Blend(value));
            }
        }
        return new TitleGradientPresentation(variants);
    }

    /// <summary>The layers <paramref name="line"/> blends with and how, as the renderers' colour-math control.</summary>
    private static ColorBlend Blend(TitleGradientScanline line)
    {
        ColorBlend blend = default;
        if (line.BlendsFloor) blend |= ColorBlend.Bg1;
        if (line.BlendsObjects) blend |= ColorBlend.Sprites;
        if (line.BlendsBackdrop) blend |= ColorBlend.Backdrop;
        if (line.Subtracts) blend |= ColorBlend.Subtract;
        return blend;
    }

    /// <summary>
    /// The authored scanline for a compiled <paramref name="line"/>: its colour, the layers it blends with and whether it
    /// subtracts. A control naming any layer or mode the title does not use is rejected rather than dropped.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="line"/> blends a layer or mode outside the title's four.</exception>
    public static TitleGradientScanline Author(TitleGradientLine line)
    {
        const ColorBlend titleBlends = ColorBlend.Bg1 | ColorBlend.Sprites |
            ColorBlend.Backdrop | ColorBlend.Subtract;
        if ((line.Control & ~titleBlends) != ColorBlend.None)
            throw new InvalidDataException($"Title gradient line blends {line.Control}, outside the floor, objects, backdrop and subtraction.");
        return new TitleGradientScanline
        {
            Red = line.Red,
            Green = line.Green,
            Blue = line.Blue,
            BlendsFloor = (line.Control & ColorBlend.Bg1) != ColorBlend.None,
            BlendsObjects = (line.Control & ColorBlend.Sprites) != ColorBlend.None,
            BlendsBackdrop = (line.Control & ColorBlend.Backdrop) != ColorBlend.None,
            Subtracts = (line.Control & ColorBlend.Subtract) != ColorBlend.None,
        };
    }

    /// <summary>Serializes and validates every gradient variant before writing any UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Authored gradient whose mutable arrays are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or scanline-value validation.</exception>
    public static void Write(Stream json, TitleGradientDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
