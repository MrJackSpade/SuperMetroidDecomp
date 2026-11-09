using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Reads and writes the installed title palette document.</summary>
public static class TitlePaletteFile
{
    /// <summary>Validates initial, ambient-animation and skip-only RGB5 colors and compiles independently owned native palette words.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>An immutable color source; ambient samples are calculated from selected paint only where the calculated output matches the supplied sample, preserving independent edits.</returns>
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
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                 TitleScreenAmbientPaletteOwners.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight =>
                    document.BabyMetroidTubeLight,
                TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays =>
                    document.FlickeringDisplays,
                _ => throw new InvalidDataException(
                    $"Unsupported title ambient palette owner {definition.Owner}."),
            };
            if (frames is null || frames.Length != definition.FrameCount)
            {
                throw new InvalidDataException(
                    $"Title palette {definition.Owner} requires exactly " +
                    $"{definition.FrameCount} frames.");
            }

            for (int frame = 0; frame < frames.Length; frame++)
            {
                PaletteRgb5[]? frameColors = frames[frame];
                if (frameColors is null || frameColors.Length != definition.ColorsPerFrame)
                {
                    throw new InvalidDataException(
                        $"Title palette {definition.Owner} frame {frame} requires exactly " +
                        $"{definition.ColorsPerFrame} colors.");
                }

                for (int index = 0; index < frameColors.Length; index++)
                {
                    PaletteRgb5? color = frameColors[index];
                    if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                        (uint)color.Blue > 31)
                    {
                        throw new InvalidDataException(
                            $"Title palette {definition.Owner} frame {frame} color {index} " +
                            "requires RGB components from 0 to 31.");
                    }

                    animatedColors.Add(
                        definition.Color(frame, index),
                        color.ToRgbColor());
                }
            }
        }

        ImmutableArray<RgbColor> screen = [.. colors];
        DropCalculatedAmbientColors(screen, animatedColors);
        return new TitlePalettePresentation(screen, animatedColors,
            PackColor(document.SkipCopyrightWhite, "skip copyright white"),
            PackColor(document.SkipCopyrightRed, "skip copyright red"));
    }

    /// <summary>Drops each selected ambient colour the shared ambient calculation already produces from <paramref name="colours"/>.</summary>
    /// <param name="colours">The selected initial colours in the cartridge's order.</param>
    /// <param name="animatedColors">Every selected ambient-program colour; calculated ones are removed.</param>
    private static void DropCalculatedAmbientColors(ImmutableArray<RgbColor> colours, Dictionary<PaletteFxColorKey, RgbColor> animatedColors)
    {
        foreach (var program in TitleScreenAmbientPaletteOwners.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (TitleAmbientColors.TryCalculate(key, colours, animatedColors, out RgbColor calculated) &&
                animatedColors[key] == calculated) animatedColors.Remove(key);
        }
    }

    /// <summary>Validates one skip-only RGB5 color before packing its CGRAM word.</summary>
    private static RgbColor PackColor(PaletteRgb5? color, string name)
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
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
