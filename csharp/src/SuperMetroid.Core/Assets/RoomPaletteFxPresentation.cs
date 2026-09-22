using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable colors for normal-room palette animations. Native timing, destinations,
/// instructions, and side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    private readonly Dictionary<ushort, ushort> colors;

    private RoomPaletteFxPresentation(Dictionary<ushort, ushort> colors) =>
        this.colors = colors;

    /// <inheritdoc />
    public bool TryReadColor(ushort pointer, out ushort color) =>
        colors.TryGetValue(pointer, out color);

    public static RoomPaletteFxPresentation Load(Stream json)
    {
        RoomPaletteFxPresentationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Room palette-FX presentation is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room palette-FX presentation JSON.", error);
        }

        if (document.Version != RoomPaletteFxPresentationFormat.Version)
        {
            throw new InvalidDataException(
                $"Room palette-FX presentation requires version " +
                $"{RoomPaletteFxPresentationFormat.Version}.");
        }

        var colors = new Dictionary<ushort, ushort>();
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase =>
                    document.NorfairForegroundAndHeatPhase,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4 =>
                    document.NorfairForegroundPalette4,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5 =>
                    document.NorfairForegroundPalette5,
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6 =>
                    document.NorfairForegroundPalette6,
                _ => throw new InvalidDataException(
                    $"Unsupported Norfair palette owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Norfair {definition.Owner}",
                frames,
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount,
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            PaletteRgb5[][]? frames = definition.Owner switch
            {
                MaridiaEnvironmentalPaletteOwner.SandPits => document.MaridiaSandPits,
                MaridiaEnvironmentalPaletteOwner.SandFalls => document.MaridiaSandFalls,
                MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls =>
                    document.MaridiaBackgroundWaterfalls,
                _ => throw new InvalidDataException(
                    $"Unsupported Maridia palette owner {definition.Owner}."),
            };
            ValidateAndCompile(
                $"Maridia {definition.Owner}",
                frames,
                definition.FrameCount,
                definition.ColorsPerFrame,
                definition.ColorPointer,
                colors);
        }
        ValidateAndCompile(
            "Wrecked Ship green lights",
            document.WreckedShipGreenLights,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer,
            colors);

        return new RoomPaletteFxPresentation(colors);
    }

    public static void Write(Stream json, RoomPaletteFxPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    private static void ValidateAndCompile(
        string owner,
        PaletteRgb5[][]? frames,
        int frameCount,
        int colorCount,
        Func<int, int, ushort> colorPointer,
        Dictionary<ushort, ushort> destination)
    {
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

                ushort pointer = colorPointer(frame, index);
                destination.Add(
                    pointer,
                    (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
            }
        }
    }
}

public sealed record RoomPaletteFxPresentationDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] NorfairForegroundAndHeatPhase { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette4 { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette5 { get; init; }
    public required PaletteRgb5[][] NorfairForegroundPalette6 { get; init; }
    public required PaletteRgb5[][] MaridiaSandPits { get; init; }
    public required PaletteRgb5[][] MaridiaSandFalls { get; init; }
    public required PaletteRgb5[][] MaridiaBackgroundWaterfalls { get; init; }
    public required PaletteRgb5[][] WreckedShipGreenLights { get; init; }
}

public static class RoomPaletteFxPresentationFormat
{
    public const string FileName = "room-palette-effects.json";
    public const int Version = 3;
}
