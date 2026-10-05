using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable authored highlight colors and durations; contains no palette addresses or audio commands.</summary>
public sealed class MapPaletteCycle
{
    private readonly Dictionary<int, byte> durationEdits = [];
    private readonly ushort[][] colors;
    private readonly Dictionary<int, ushort> reverseColorEdits = [];

    /// <summary>
    /// Native $82:C10C pauses fifteen ticks at the loop origin and advances other
    /// highlight phases every three ticks. Independently edited frame holds remain exact.
    /// </summary>
    private static byte NativeDuration(int frame) => frame == 0 ? (byte)15 : (byte)3;

    private MapPaletteCycle(byte[] durations, ushort[][] suppliedColors)
    {
        FrameCount = durations.Length;
        for (int frame = 0; frame < FrameCount; frame++)
            if (durations[frame] != NativeDuration(frame)) durationEdits.Add(frame, durations[frame]);
        // The fourteen native frames traverse eight phases forward then backward.
        // Retain the independent phase artwork; only the reverse traversal is calculated.
        int phaseCount = FrameCount == 14 ? 8 : FrameCount;
        colors = suppliedColors[..phaseCount];
        for (int frame = phaseCount; frame < FrameCount; frame++)
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            if (suppliedColors[frame][color] != colors[Phase(frame)][color])
                reverseColorEdits.Add(frame * MapPaletteCycleFormat.ColorCount + color, suppliedColors[frame][color]);
    }

    public int FrameCount { get; }

    private int Phase(int frame) => FrameCount == 14 ? Math.Min(frame, 14 - frame) : frame;

    private void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
    }

    public byte Duration(int frame)
    {
        CheckFrame(frame);
        return durationEdits.TryGetValue(frame, out byte duration) ? duration : NativeDuration(frame);
    }

    public void Apply(SnesCgram destination, int frame, int firstColor)
    {
        CheckFrame(frame);
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            destination.SetColor(firstColor + color,
                reverseColorEdits.TryGetValue(frame * MapPaletteCycleFormat.ColorCount + color, out ushort edited)
                    ? edited : colors[Phase(frame)][color]);
    }

    public static MapPaletteCycle Load(Stream json)
    {
        MapPaletteCycleDocument document = JsonAssetDocument.Read<MapPaletteCycleDocument>(
            json, MapPresentationFormat.JsonOptions, "map highlight cycle");
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
