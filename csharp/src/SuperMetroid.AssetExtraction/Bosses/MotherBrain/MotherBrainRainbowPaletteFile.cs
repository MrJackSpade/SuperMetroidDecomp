using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain rainbow palette document.</summary>
public static class MotherBrainRainbowPaletteFile
{
    /// <summary>Installed editable JSON filename for rainbow, drain/revival, fake-death, restoration, and beam-backdrop colors.</summary>
    public const string FileName = "mother-brain-rainbow-palette.json";

    /// <summary>Supported schema version, requiring the brain-only fake-death sequence alongside every other color family.</summary>
    public const int Version = 3;

    /// <summary>Loads and compiles bounded RGB5 rainbow, drain, revival, restoration, and beam-backdrop colors; validates frame geometry and channels without importing engine timers or signed loop terminators.</summary>
    /// <param name="json">Caller-owned <c>mother-brain-rainbow-palette.json</c> stream, read from its current position and left open.</param>
    /// <returns>Compiled colors retaining independent edits while sharing calculated native color relationships when their supplied values agree.</returns>
    public static MotherBrainRainbowPalettePresentation Read(Stream json)
    {
        MotherBrainRainbowPaletteDocument document = JsonAssetDocument.Read<MotherBrainRainbowPaletteDocument>(
            json, ArtworkJson.Options, "Mother Brain rainbow palette");
        if (document.Version != Version)
            throw new InvalidDataException("Unsupported Mother Brain rainbow palette version.");
        return MotherBrainRainbowPaletteRecognition.Create(new MotherBrainRainbowPaletteColours
        {
            Rainbow = CompileFrames(document.Rainbow, MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Rainbow)),
            ToGrey = CompileFrames(document.ToGrey, MotherBrainRainbowPaletteGeometry.GreyFrameCount,
                MotherBrainRainbowPaletteGeometry.DrainedColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.ToGrey)),
            FromGrey = CompileFrames(document.FromGrey, MotherBrainRainbowPaletteGeometry.GreyFrameCount,
                MotherBrainRainbowPaletteGeometry.RevivalColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.FromGrey)),
            FakeDeathToGrey = CompileFakeDeathFrames(document.FakeDeathToGrey),
            Normal = CompileFrames([document.Normal], 1, MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Normal))[0],
            BeamInitial = CompileColor(document.BeamInitial, nameof(document.BeamInitial)),
            BeamCycle = CompileColors(document.BeamCycle, MotherBrainRainbowPaletteGeometry.BeamCycleColorCount,
                nameof(document.BeamCycle)),
        });
    }

    /// <summary>Requires eight three-ink brain frames, with no rear-leg or trailing color.</summary>
    private static RgbColor[][] CompileFakeDeathFrames(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != MotherBrainFakeDeathPaletteDefinitions.FrameCount)
            throw new InvalidDataException("Mother Brain fake-death fade requires eight frames.");
        var frames = new RgbColor[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
            frames[frame] = CompileColors(source[frame],
                MotherBrainFakeDeathPaletteDefinitions.ColorCount, $"fake-death frame {frame}");
        return frames;
    }

    /// <summary>Validates palette geometry and whether this sequence carries a trailing WRAM word.</summary>
    private static MotherBrainPaletteFrameColours[] CompileFrames(MotherBrainRainbowPaletteFrameDocument[]? source,
        int count, int bodyCount, int legCount, MotherBrainTrailingColor trailing, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Mother Brain {name} requires {count} frames.");
        var frames = new MotherBrainPaletteFrameColours[count];
        for (int frame = 0; frame < count; frame++)
        {
            MotherBrainRainbowPaletteFrameDocument? item = source[frame];
            if (item is null || (item.TrailingColor is null) != (trailing == MotherBrainTrailingColor.Absent))
                throw new InvalidDataException($"Mother Brain {name} frame {frame} has an invalid trailing color.");
            frames[frame] = new MotherBrainPaletteFrameColours(
                CompileColors(item.Body, bodyCount, $"{name} frame {frame} body"),
                CompileColors(item.BackLegs, legCount, $"{name} frame {frame} back legs"),
                item.TrailingColor is null ? null : CompileColor(item.TrailingColor,
                    $"{name} frame {frame} trailing color"));
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
    /// <param name="document">Bounded RGB5 palette and beam-color sequences to serialize.</param>
    public static void Write(Stream json, MotherBrainRainbowPaletteDocument document)
    {
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}
