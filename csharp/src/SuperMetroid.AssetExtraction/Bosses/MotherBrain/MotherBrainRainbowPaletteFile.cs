using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Reads and writes Mother Brain's rainbow palette document, which is import data (#1637): the extraction writes it to the
/// map presentation's import-data directory and only the import reads it.
/// </summary>
public static class MotherBrainRainbowPaletteFile
{
    /// <summary>The import-data JSON filename for rainbow, drain/revival, fake-death and restoration colors.</summary>
    public const string FileName = "mother-brain-rainbow-palette.json";

    /// <summary>Supported schema version: 4 holds no beam-backdrop colour, which is installed by purpose (#1637).</summary>
    public const int Version = 4;

    /// <summary>Loads and compiles bounded RGB5 rainbow, drain, revival and restoration colors; validates frame geometry and channels without importing engine timers.</summary>
    /// <param name="json">Caller-owned <c>mother-brain-rainbow-palette.json</c> stream, read from its current position and left open.</param>
    /// <returns>Every installed phase, kept as installed.</returns>
    public static InstalledMotherBrainRainbowPalette Read(Stream json)
    {
        MotherBrainRainbowPaletteDocument document = JsonAssetDocument.Read<MotherBrainRainbowPaletteDocument>(
            json, ArtworkJson.Options, "Mother Brain rainbow palette");
        if (document.Version != Version)
            throw new InvalidDataException("Unsupported Mother Brain rainbow palette version.");
        return new InstalledMotherBrainRainbowPalette(
            CompileFrames(document.Rainbow, MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteGeometry.ColorCount,
                MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Rainbow)),
            new InstalledMotherBrainRainbowPalette.PaletteFade(CompileFrames(document.ToGrey, MotherBrainRainbowPaletteGeometry.GreyFrameCount,
                MotherBrainRainbowPaletteGeometry.DrainedColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.ToGrey))),
            new InstalledMotherBrainRainbowPalette.PaletteFade(CompileFrames(document.FromGrey, MotherBrainRainbowPaletteGeometry.GreyFrameCount,
                MotherBrainRainbowPaletteGeometry.RevivalColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.FromGrey))),
            new InstalledMotherBrainRainbowPalette.PaletteFade(CompileFakeDeathFrames(document.FakeDeathToGrey)),
            CompileFrames([document.Normal], 1, MotherBrainRainbowPaletteGeometry.ColorCount,
                MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Normal))[0]);
    }

    /// <summary>Requires eight three-ink brain frames, with no rear-leg or trailing color.</summary>
    private static InstalledMotherBrainRainbowPalette.PaletteFrame[] CompileFakeDeathFrames(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != MotherBrainFakeDeathPaletteDefinitions.FrameCount)
            throw new InvalidDataException("Mother Brain fake-death fade requires eight frames.");
        var frames = new InstalledMotherBrainRainbowPalette.PaletteFrame[source.Length];
        for (int frame = 0; frame < source.Length; frame++)
            frames[frame] = new(CompileColors(source[frame],
                MotherBrainRainbowPaletteGeometry.FakeDeathColors, $"fake-death frame {frame}"), []);
        return frames;
    }

    /// <summary>Validates palette geometry and whether this sequence carries a trailing WRAM word.</summary>
    private static InstalledMotherBrainRainbowPalette.PaletteFrame[] CompileFrames(MotherBrainRainbowPaletteFrameDocument[]? source,
        int count, int bodyCount, int legCount, MotherBrainTrailingColor trailing, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} frames.");
        var frames = new InstalledMotherBrainRainbowPalette.PaletteFrame[count];
        for (int frame = 0; frame < count; frame++)
        {
            MotherBrainRainbowPaletteFrameDocument? item = source[frame];
            if (item is null || (item.TrailingColor is null) != (trailing == MotherBrainTrailingColor.Absent))
                throw new InvalidDataException($"Mother Brain {name} frame {frame} has an invalid trailing color.");
            if (item.TrailingColor is not null)
                _ = CompileColor(item.TrailingColor, $"{name} frame {frame} trailing color");
            frames[frame] = new InstalledMotherBrainRainbowPalette.PaletteFrame(
                CompileColors(item.Body, bodyCount, $"{name} frame {frame} body"),
                CompileColors(item.BackLegs, legCount, $"{name} frame {frame} back legs"));
        }
        return frames;
    }

    /// <summary>Checks an authored row's exact length before converting its RGB5 colors.</summary>
    private static RgbColor[] CompileColors(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} colors.");
        var colors = new RgbColor[count];
        for (int color = 0; color < count; color++)
            colors[color] = CompileColor(source[color], $"{name} color {color}");
        return colors;
    }

    /// <summary>Rejects absent colors and channels outside the five-bit hardware range.</summary>
    private static RgbColor CompileColor(PaletteRgb5? color, string name)
    {
        if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"Mother Brain {name} requires RGB5 components.");
        return color.ToRgbColor();
    }

    /// <summary>Serializes UTF-8 palette JSON and writes it to the destination.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Bounded RGB5 palette sequences to serialize.</param>
    public static void Write(Stream json, MotherBrainRainbowPaletteDocument document)
    {
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}
