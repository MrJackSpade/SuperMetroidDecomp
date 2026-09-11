using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable authored highlight colors and durations; contains no palette addresses or audio commands.</summary>
public sealed class MapPaletteCycle
{
    private readonly byte[] durations;
    private readonly ushort[][] colors;
    private MapPaletteCycle(byte[] durations, ushort[][] colors) { this.durations = durations; this.colors = colors; }
    public int FrameCount => durations.Length;
    public byte Duration(int frame) => durations[frame];
    public void Apply(SnesCgram destination, int frame, int firstColor)
    {
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            destination.SetColor(firstColor + color, colors[frame][color]);
    }

    public static MapPaletteCycle Load(Stream json)
    {
        MapPaletteCycleDocument document;
        try { document = JsonSerializer.Deserialize<MapPaletteCycleDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map highlight cycle is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map highlight cycle JSON.", error); }
        if (document.Version != MapPaletteCycleFormat.Version || document.Frames is null ||
            document.Frames.Length is < 1 or > MapPaletteCycleFormat.MaximumFrames)
            throw new InvalidDataException("Map highlight cycle requires version 1 and 1-255 frames.");
        var durations = new byte[document.Frames.Length];
        var colors = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < durations.Length; frame++)
        {
            var entry = document.Frames[frame];
            if (entry is null || entry.DurationTicks is < 1 or > MapPaletteCycleFormat.MaximumDuration ||
                entry.Colors is null || entry.Colors.Length != MapPaletteCycleFormat.ColorCount)
                throw new InvalidDataException($"Map highlight frame {frame} requires 1-254 ticks and sixteen colors.");
            durations[frame] = (byte)entry.DurationTicks;
            colors[frame] = new ushort[entry.Colors.Length];
            for (int color = 0; color < entry.Colors.Length; color++)
            {
                var rgb = entry.Colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Map highlight frame {frame}, color {color} requires RGB components from 0 to 31.");
                colors[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new(durations, colors);
    }

    public static void Write(Stream json, MapPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MapPaletteCycleDocument
{
    public required int Version { get; init; }
    public required MapPaletteCycleFrame[] Frames { get; init; }
}
public sealed record MapPaletteCycleFrame
{
    public required int DurationTicks { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}
/// <summary>Native-precision RGB components, not a packed hardware word.</summary>
public sealed record PaletteRgb5
{
    public required int Red { get; init; }
    public required int Green { get; init; }
    public required int Blue { get; init; }
}
public static class MapPaletteCycleFormat
{
    public const string FileName = "map-highlight-cycle.json";
    public const int Version = 1;
    public const int ColorCount = 16;
    public const int MaximumFrames = 255;
    public const int MaximumDuration = 254;
}
