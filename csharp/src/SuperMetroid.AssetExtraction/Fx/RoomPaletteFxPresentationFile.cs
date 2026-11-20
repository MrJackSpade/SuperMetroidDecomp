using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads and writes the installed room palette-effect colour document.</summary>
public static class RoomPaletteFxPresentationFile
{
    /// <summary>Validates the installed palette-FX frames and keeps every RGB5 colour as installed, by the record colour it is.</summary>
    /// <param name="json">The UTF-8 JSON presentation stream with the required frame counts and RGB components from zero through thirty-one.</param>
    /// <returns>A color source holding the complete installed colour set.</returns>
    /// <exception cref="InvalidDataException">The schema version, required frame geometry, or a color component is invalid.</exception>
    public static RoomPaletteFxPresentation Read(Stream json)
    {
        RoomPaletteFxPresentationDocument document = JsonAssetDocument.Read<RoomPaletteFxPresentationDocument>(
            json, ArtworkJson.Options, "room palette-FX presentation");

        if (document.Version != RoomPaletteFxPresentationFormat.Version)
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<PaletteFxColorKey, RgbColor>();
        if (document.Programs is null || document.Programs.Count != RoomPaletteFxPrograms.All.Count)
            throw new InvalidDataException($"Room palette-FX presentation requires exactly {RoomPaletteFxPrograms.All.Count} colour programs.");
        foreach (RoomPaletteFxProgram program in RoomPaletteFxPrograms.All)
        {
            document.Programs.TryGetValue(program.Key, out PaletteRgb5[][]? frames);
            foreach (RoomPaletteFxRun run in program.Runs)
                ValidateAndCompile(run.Owner, frames, run.Frames, colors);
        }
        return new RoomPaletteFxPresentation(colors);
    }

    /// <summary>Serializes a current-version presentation document to the destination.</summary>
    /// <param name="json">The stream that receives the UTF-8 JSON document.</param>
    /// <param name="document">The complete editable color document; animation timing and destinations remain defined by engine mechanics.</param>
    public static void Write(Stream json, RoomPaletteFxPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document,
            ArtworkJson.Options);
        json.Write(bytes);
    }

    /// <summary>Validates one color family's frame geometry and stores each RGB5 sample by its record color.</summary>
    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        PaletteFxColorFrames colorFrames,
        Dictionary<PaletteFxColorKey, RgbColor> destination)
    {
        int frameCount = colorFrames.FrameCount;
        int colorCount = colorFrames.ColorsPerFrame;
        if (frames is null || frames.Length != frameCount)
        {
            throw new InvalidDataException(
                $"Room palette-FX {owner} requires exactly {frameCount} frames.");
        }

        for (int frame = 0; frame < frames.Length; frame++)
        {
            PaletteRgb5[]? frameColors = frames[frame];
            if (frameColors is null || frameColors.Length != colorCount)
            {
                throw new InvalidDataException(
                    $"Room palette-FX {owner} frame {frame} requires exactly " +
                    $"{colorCount} colors.");
            }

            for (int index = 0; index < frameColors.Length; index++)
            {
                PaletteRgb5? color = frameColors[index];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                    (uint)color.Blue > 31)
                {
                    throw new InvalidDataException(
                        $"Room palette-FX {owner} frame {frame} color {index} " +
                        "requires RGB components from 0 to 31.");
                }

                destination.Add(
                    colorFrames.Color(frame, index),
                    color.ToRgbColor());
            }
        }
    }
}
