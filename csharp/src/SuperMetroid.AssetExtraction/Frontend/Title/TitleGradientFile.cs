using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Reads and writes the installed title gradient: sixteen zoom variants, each described by its stops (#1468).</summary>
public static class TitleGradientFile
{
    /// <summary>Installed JSON filename selected by the title presentation catalog.</summary>
    public const string FileName = "title-gradient.json";

    /// <summary>
    /// Revision 3 of the sixteen-variant schema (#1468): each variant's stops, each the line its band begins on, its RGB5
    /// colour, the layers it blends with and whether it subtracts. Revision 2 stored every screen line; revision 1 also
    /// stored the blend as a control byte.
    /// </summary>
    public const int Version = 3;

    /// <summary>Validates all sixteen authored zoom variants and compiles their stops.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open for the caller.</param>
    /// <returns>An immutable presentation whose gradients do not retain the document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or variant ordering/count is wrong, or a variant's stops do not start on line 0, run top to bottom on the screen and hold RGB5 colours.</exception>
    public static TitleGradientPresentation Read(Stream json)
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

        if (document.Version != Version ||
            document.Variants is null ||
            document.Variants.Length != TitleGradientFormat.VariantCount)
        {
            throw new InvalidDataException(
                $"Title gradient requires version {Version} and " +
                $"{TitleGradientFormat.VariantCount} zoom variants.");
        }

        var variants = new ScreenGradient[TitleGradientFormat.VariantCount];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            TitleGradientVariant? source = document.Variants[variant];
            if (source is null || source.ZoomHighNibble != variant || source.Stops is null || source.Stops.Length == 0)
            {
                throw new InvalidDataException(
                    $"Title gradient variant {variant} must identify nibble {variant} and contain stops.");
            }

            var stops = ImmutableArray.CreateBuilder<GradientStop>(source.Stops.Length);
            foreach (TitleGradientStop? value in source.Stops)
            {
                if (value is null || value.Red is < 0 or > RgbColor.MaxChannel || value.Green is < 0 or > RgbColor.MaxChannel ||
                    value.Blue is < 0 or > RgbColor.MaxChannel)
                {
                    throw new InvalidDataException(
                        $"Title gradient variant {variant}, stop {stops.Count} requires an RGB5 colour.");
                }
                bool placed = stops.Count == 0 ? value.Line == 0 : value.Line > stops[^1].Line && value.Line < DisplayScreen.Height;
                if (!placed)
                {
                    throw new InvalidDataException(
                        $"Title gradient variant {variant}, stop {stops.Count} must begin on line 0 or below the stop before it, on the screen.");
                }
                stops.Add(new GradientStop(value.Line, new RgbColor(value.Red, value.Green, value.Blue), Blend(value)));
            }
            variants[variant] = new ScreenGradient(stops.MoveToImmutable(), DisplayScreen.Height);
        }
        return new TitleGradientPresentation(variants);
    }

    /// <summary>The layers <paramref name="stop"/> blends with and how, as the renderers' colour-math control.</summary>
    private static ColorBlend Blend(TitleGradientStop stop)
    {
        ColorBlend blend = default;
        if (stop.BlendsFloor) blend |= ColorBlend.Bg1;
        if (stop.BlendsObjects) blend |= ColorBlend.Sprites;
        if (stop.BlendsBackdrop) blend |= ColorBlend.Backdrop;
        if (stop.Subtracts) blend |= ColorBlend.Subtract;
        return blend;
    }

    /// <summary>
    /// The authored stop for a compiled <paramref name="stop"/>: its line, its colour, the layers it blends with and
    /// whether it subtracts. A blend naming any layer or mode the title does not use is rejected rather than dropped.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="stop"/> blends a layer or mode outside the title's four.</exception>
    public static TitleGradientStop Author(GradientStop stop)
    {
        const ColorBlend titleBlends = ColorBlend.Bg1 | ColorBlend.Sprites |
            ColorBlend.Backdrop | ColorBlend.Subtract;
        if ((stop.Blend & ~titleBlends) != ColorBlend.None)
            throw new InvalidDataException($"Title gradient stop blends {stop.Blend}, outside the floor, objects, backdrop and subtraction.");
        return new TitleGradientStop
        {
            Line = stop.Line,
            Red = stop.Colour.Red,
            Green = stop.Colour.Green,
            Blue = stop.Colour.Blue,
            BlendsFloor = (stop.Blend & ColorBlend.Bg1) != ColorBlend.None,
            BlendsObjects = (stop.Blend & ColorBlend.Sprites) != ColorBlend.None,
            BlendsBackdrop = (stop.Blend & ColorBlend.Backdrop) != ColorBlend.None,
            Subtracts = (stop.Blend & ColorBlend.Subtract) != ColorBlend.None,
        };
    }

    /// <summary>Serializes and validates every gradient variant before writing any UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Authored gradient whose mutable arrays are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Read"/>'s schema or stop validation.</exception>
    public static void Write(Stream json, TitleGradientDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
