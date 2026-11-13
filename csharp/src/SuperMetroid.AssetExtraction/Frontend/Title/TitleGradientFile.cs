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
    /// Revision 4 of the sixteen-variant schema (#1468): the variants in zoom-bucket order, each by its stops, each the line
    /// its band begins on, its RGB5 colour and the name of its blend (#1709). Revision 3 stored the blend as the layers it
    /// blends with and whether it subtracts, and keyed each variant by its bucket; revision 2 stored every screen line;
    /// revision 1 also stored the blend as a control byte.
    /// </summary>
    public const int Version = 4;

    /// <summary>Every gradient blend by its exact name.</summary>
    private static readonly Dictionary<string, GradientBlend> BlendsByName =
        Enum.GetValues<GradientBlend>().ToDictionary(blend => blend.ToString(), StringComparer.Ordinal);

    /// <summary>Validates all sixteen authored zoom variants and compiles their stops.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open for the caller.</param>
    /// <returns>An immutable presentation whose gradients do not retain the document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or variant count is wrong, or a variant's stops do not start on line 0, run top to bottom on the screen, hold RGB5 colours and name a gradient blend.</exception>
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
            if (source?.Stops is null || source.Stops.Length == 0)
                throw new InvalidDataException($"Title gradient variant {variant} must contain stops.");

            var stops = ImmutableArray.CreateBuilder<GradientStop>(source.Stops.Length);
            foreach (TitleGradientStop? value in source.Stops)
            {
                if (value is null || value.Red is < 0 or > FadeLevelColour.FullLevel || value.Green is < 0 or > FadeLevelColour.FullLevel ||
                    value.Blue is < 0 or > FadeLevelColour.FullLevel)
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
                stops.Add(new GradientStop(value.Line, new FadeLevelColour(value.Red, value.Green, value.Blue).ToColour(), Blend(value)));
            }
            variants[variant] = new ScreenGradient(stops.MoveToImmutable(), DisplayScreen.Height);
        }
        return new TitleGradientPresentation(variants);
    }

    /// <summary>
    /// The gradient blend <paramref name="stop"/> names (#1514, #1709): its colour subtracted from the floor and the backdrop,
    /// darkening them, or added to the floor, the backdrop and the sprites, brightening them.
    /// </summary>
    /// <exception cref="InvalidDataException">The stop names no gradient blend.</exception>
    private static GradientBlend Blend(TitleGradientStop stop) =>
        stop.Blend is not null && BlendsByName.TryGetValue(stop.Blend, out GradientBlend blend)
            ? blend
            : throw new InvalidDataException(
                $"Title gradient stop at line {stop.Line} names blend '{stop.Blend}', which neither darkens the floor and the backdrop nor brightens them and the sprites.");

    /// <summary>The authored stop for a compiled <paramref name="stop"/>: its line, its colour and the name of its blend.</summary>
    /// <exception cref="InvalidDataException"><paramref name="stop"/>'s blend is not a defined gradient blend.</exception>
    public static TitleGradientStop Author(GradientStop stop) => new()
    {
        Line = stop.Line,
        Red = stop.Colour.FadeLevels.Red,
        Green = stop.Colour.FadeLevels.Green,
        Blue = stop.Colour.FadeLevels.Blue,
        Blend = Enum.IsDefined(stop.Blend)
            ? stop.Blend.ToString()
            : throw new InvalidDataException($"Title gradient stop blends in undefined way {stop.Blend}."),
    };

    /// <summary>Serializes every gradient variant as UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Authored gradient whose mutable arrays are read for serialization, not retained by the writer.</param>
    public static void Write(Stream json, TitleGradientDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
