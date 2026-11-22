using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Reads and writes the import's title palette document.</summary>
public static partial class TitlePaletteFile
{
    /// <summary>Validates the import's initial, ambient-animation and skip-only RGB5 colours and keeps every one.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable colour source holding every initial and ambient colour the import reads.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version or array dimensions are wrong, or a required color is null or has a channel outside 0..31.</exception>
    public static TitlePalettePresentation Read(Stream json)
    {
        TitlePaletteDocument document = JsonAssetDocument.Read<TitlePaletteDocument>(
            json, ArtworkJson.Options, "title palette presentation");

        if (document.Version != TitlePaletteFormat.Version ||
            document.Colors is null ||
            document.Colors.Length != ScreenColourImage.ColorCount)
        {
            throw new InvalidDataException(
                $"Title palette requires version {TitlePaletteFormat.Version} and " +
                $"{ScreenColourImage.ColorCount} RGB5 colors.");
        }

        var colors = new RgbColor[ScreenColourImage.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
            {
                throw new InvalidDataException(
                    $"Title palette color {index} requires RGB components from 0 to 31.");
            }
            colors[index] = color.ToRgbColor();
        }
        var animatedColors = new Dictionary<PaletteFxColorKey, RgbColor>();
        ReadAnimated(TitleScreenAmbientPaletteFxColorDefinitions.TubeLight, document.BabyMetroidTubeLight, animatedColors);
        ReadAnimated(TitleScreenAmbientPaletteFxColorDefinitions.Displays, document.FlickeringDisplays, animatedColors);

        ImmutableArray<RgbColor> screen = [.. colors];
        return new TitlePalettePresentation(screen, animatedColors,
            SkipColour(document.SkipCopyrightWhite, "skip copyright white"),
            SkipColour(document.SkipCopyrightTint, "skip copyright tint"));
    }

    /// <summary>
    /// Validates the document's records of one looping ambient program, <paramref name="frames"/>, and adds each colour to
    /// <paramref name="animatedColors"/> under its key in <paramref name="definition"/>.
    /// </summary>
    private static void ReadAnimated(TitleScreenAmbientPaletteFxColorDefinition definition, PaletteRgb5[][]? frames,
        Dictionary<PaletteFxColorKey, RgbColor> animatedColors)
    {
        if (frames is null || frames.Length != definition.FrameCount)
        {
            throw new InvalidDataException(
                $"Title palette {definition.Effect} requires exactly " +
                $"{definition.FrameCount} frames.");
        }

        for (int frame = 0; frame < frames.Length; frame++)
        {
            PaletteRgb5[]? frameColors = frames[frame];
            if (frameColors is null || frameColors.Length != definition.ColorsPerFrame)
            {
                throw new InvalidDataException(
                    $"Title palette {definition.Effect} frame {frame} requires exactly " +
                    $"{definition.ColorsPerFrame} colors.");
            }

            for (int index = 0; index < frameColors.Length; index++)
            {
                PaletteRgb5? color = frameColors[index];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                    (uint)color.Blue > 31)
                {
                    throw new InvalidDataException(
                        $"Title palette {definition.Effect} frame {frame} color {index} " +
                        "requires RGB components from 0 to 31.");
                }

                animatedColors.Add(
                    definition.Color(frame, index),
                    color.ToRgbColor());
            }
        }
    }

    /// <summary>Validates one skip-only RGB5 colour and converts it to its RGB colour.</summary>
    private static RgbColor SkipColour(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
            (uint)color.Blue > 31)
            throw new InvalidDataException($"Title {name} requires RGB components from 0 to 31.");
        return color.ToRgbColor();
    }

    /// <summary>Serializes the complete title palette document as UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="document">Editable RGB5 collections read for serialization, not retained by the writer.</param>
    public static void Write(Stream json, TitlePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Title.TitlePaletteDocument>(ArtworkJson.Options));
        json.Write(bytes);
    }
}
