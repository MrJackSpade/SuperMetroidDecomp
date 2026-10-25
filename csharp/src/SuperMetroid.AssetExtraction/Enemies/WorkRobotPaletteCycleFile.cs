using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads and writes the installed Work Robot palette cycle document.</summary>
public static class WorkRobotPaletteCycleFile
{
    /// <summary>Loads and validates the six four-color Work Robot frames from JSON.</summary>
    /// <param name="json">Caller-owned stream containing the palette-cycle document.</param>
    /// <returns>The compiled palette cycle.</returns>
    public static WorkRobotPaletteCycle Read(Stream json)
    {
        Ensure.NotNull(json);
        WorkRobotPaletteCycleDocument document;
        document = JsonAssetDocument.Read<WorkRobotPaletteCycleDocument>(json, ArtworkJson.Options, "Work Robot palette cycle");

        if (document.Version != WorkRobotPaletteCycleFormat.Version ||
            document.Frames is null ||
            document.Frames.Length != WorkRobotPaletteTimingDefinitions.RecordCount)
            throw new InvalidDataException("Work Robot palette cycle requires six version-one frames.");

        var compiled = new Bgr555[WorkRobotPaletteTimingDefinitions.RecordCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != WorkRobotPaletteDefinitions.ColorCount)
                throw new InvalidDataException($"Work Robot palette frame {frame} requires four RGB5 colors.");
            compiled[frame] = new Bgr555[source.Length];
            for (int color = 0; color < source.Length; color++)
            {
                PaletteRgb5? rgb = source[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Work Robot palette frame {frame} color {color} requires RGB5 channels 0..31.");
                compiled[frame][color] = rgb.ToBgr555();
            }
        }
        return new WorkRobotPaletteCycle(compiled);
    }

    /// <summary>Validates and serializes a Work Robot palette-cycle document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing all six ordered frames.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(WorkRobotPaletteCycleDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));
}
