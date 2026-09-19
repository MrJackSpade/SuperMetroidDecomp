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
    public ReadOnlySpan<TitleGradientLine> Resolve(ushort zoom) =>
        variants[(zoom & 0xf0) >> 4];

    public static TitleGradientPresentation Load(Stream json)
    {
        TitleGradientDocument document;
        try
        {
            document = JsonSerializer.Deserialize<TitleGradientDocument>(
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

    public static void Write(Stream json, TitleGradientDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record TitleGradientDocument
{
    public required int Version { get; init; }
    public required TitleGradientVariant[] Variants { get; init; }
}

public sealed record TitleGradientVariant
{
    /// <summary>Zero through fifteen; native selects this with zoom bits four through seven.</summary>
    public required int ZoomHighNibble { get; init; }
    public required TitleGradientScanline[] Lines { get; init; }
}

public sealed record TitleGradientScanline
{
    public required int Red { get; init; }
    public required int Green { get; init; }
    public required int Blue { get; init; }
    public required int ColorMathControl { get; init; }
}

public static class TitleGradientFormat
{
    public const string FileName = "title-gradient.json";
    public const int Version = 1;
    public const int VariantCount = 16;
}
