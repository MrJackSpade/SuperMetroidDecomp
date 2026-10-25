using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain rainbow palette document.</summary>
public static class MotherBrainRainbowPaletteFile
{
    /// <summary>Loads and compiles bounded RGB5 rainbow, drain, revival, restoration, and beam-backdrop colors; validates frame geometry and channels without importing engine timers or signed loop terminators.</summary>
    /// <param name="json">Caller-owned <c>mother-brain-rainbow-palette.json</c> stream, read from its current position and left open.</param>
    /// <param name="currentStock">Verified current presentation supplying only the missing fake-death fade when loading legacy version 2; without it, version 3 is required.</param>
    /// <returns>Compiled colors retaining independent edits while sharing calculated native color relationships when their supplied values agree.</returns>
    public static MotherBrainRainbowPalettePresentation Read(Stream json,
        MotherBrainRainbowPalettePresentation? currentStock = null)
    {
        MotherBrainRainbowPaletteDocument document = JsonAssetDocument.Read<MotherBrainRainbowPaletteDocument>(
            json, ArtworkJson.Options, "Mother Brain rainbow palette");
        var version = (MotherBrainRainbowPaletteVersion)document.Version;
        if (version != MotherBrainRainbowPaletteVersion.Current &&
            !(version == MotherBrainRainbowPaletteVersion.PreFakeDeath && currentStock is not null))
            throw new InvalidDataException("Unsupported Mother Brain rainbow palette version.");
        return MotherBrainRainbowPalettePresentation.Create(new MotherBrainRainbowPaletteColours
        {
            Rainbow = CompileFrames(document.Rainbow, MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Rainbow)),
            ToGrey = CompileFrames(document.ToGrey, MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteDefinitions.DrainedColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.ToGrey)),
            FromGrey = CompileFrames(document.FromGrey, MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteDefinitions.RevivalColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, MotherBrainTrailingColor.Required, nameof(document.FromGrey)),
            // A pre-fake-death override inherits that fade from the current stock the version check required.
            FakeDeathToGrey = version == MotherBrainRainbowPaletteVersion.PreFakeDeath
                ? null
                : CompileFakeDeathFrames(document.FakeDeathToGrey),
            Normal = CompileFrames([document.Normal], 1, MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, MotherBrainTrailingColor.Absent, nameof(document.Normal))[0],
            BeamInitial = CompileColor(document.BeamInitial, nameof(document.BeamInitial)),
            BeamCycle = CompileColors(document.BeamCycle, MotherBrainRainbowPaletteFormat.BeamCycleColorCount,
                nameof(document.BeamCycle)),
        }, currentStock);
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

    /// <summary>Serializes UTF-8 palette JSON and validates it without a legacy fallback before writing any bytes; consequently the document must satisfy the complete version-3 schema.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Bounded RGB5 palette and beam-color sequences to serialize.</param>
    public static void Write(Stream json, MotherBrainRainbowPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
