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

    /// <summary>Number of selected ordered color frames, one through 255; the animation owner, not this resource, advances and wraps the frame cursor.</summary>
    public int FrameCount { get; }

    private int Phase(int frame) => FrameCount == 14 ? Math.Min(frame, 14 - frame) : frame;

    private void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
    }

    /// <summary>Returns the authored hold for one color frame in map-palette animation updates.</summary>
    /// <param name="frame">Zero-based selected frame index, strictly below <see cref="FrameCount"/>; not automatically wrapped.</param>
    /// <returns>A hold of one through 254 ticks; stock frame zero holds fifteen ticks and other stock frames hold three.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public byte Duration(int frame)
    {
        CheckFrame(frame);
        return durationEdits.TryGetValue(frame, out byte duration) ? duration : NativeDuration(frame);
    }

    /// <summary>Copies the selected frame's sixteen BGR555 colors into consecutive live CGRAM slots without advancing time or requesting audio.</summary>
    /// <param name="destination">Live palette state modified in place.</param>
    /// <param name="frame">Zero-based frame index, strictly below <see cref="FrameCount"/>.</param>
    /// <param name="firstColor">CGRAM color index of the first destination, zero through 240; map consumers use 176 for colors 176-191, not a byte address.</param>
    /// <remarks>Writes all sixteen colors including the row's first color. Destination bounds are checked per write, so an invalid range can leave earlier colors already written.</remarks>
    /// <exception cref="IndexOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A destination color index falls outside CGRAM's 256 slots.</exception>
    public void Apply(SnesCgram destination, int frame, int firstColor)
    {
        CheckFrame(frame);
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            destination.SetColor(firstColor + color,
                reverseColorEdits.TryGetValue(frame * MapPaletteCycleFormat.ColorCount + color, out ushort edited)
                    ? edited : colors[Phase(frame)][color]);
    }

    /// <summary>Compiles an editable ordered color-and-duration document into an immutable map-highlight cycle.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A cycle owning copied native-precision colors and holds; it contains no playback timer, CGRAM destination, or loop-sound command.</returns>
    /// <remarks>Stock extraction reads timing at $82:C10C and colors at $82:A987. A fourteen-frame cycle shares matching reverse phases while retaining independently edited reverse colors exactly; other lengths preserve the supplied frame sequence directly.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, version, frame count, tick durations, sixteen-color frame shape, or RGB5 components are invalid.</exception>
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

    /// <summary>Serializes and validates a complete highlight cycle before writing any UTF-8 document bytes to the destination.</summary>
    /// <param name="json">Caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Ordered selected frames satisfying <see cref="Load"/>'s version, duration, count, and color requirements.</param>
    /// <exception cref="InvalidDataException">The document is null or fails highlight-cycle validation.</exception>
    public static void Write(Stream json, MapPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable map-highlight cycle schema, containing only authored durations and palette rows rather than an executable palette program.</summary>
public sealed record MapPaletteCycleDocument
{
    /// <summary>Schema revision; loading requires <see cref="MapPaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one through 255 frames in playback order; copied into compiled values when loaded.</summary>
    public required MapPaletteCycleFrame[] Frames { get; init; }
}
/// <summary>One highlight phase's palette row and update-counted hold.</summary>
public sealed record MapPaletteCycleFrame
{
    /// <summary>Number of map-palette animation ticks to hold this frame, one through 254; not elapsed milliseconds or a native callback opcode.</summary>
    public required int DurationTicks { get; init; }
    /// <summary>Exactly sixteen native-precision RGB colors in destination palette-row order, including its first color.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
/// <summary>Native-precision RGB components, not a packed hardware word.</summary>
public sealed record PaletteRgb5
{
    /// <summary>Red intensity, zero through 31, encoded in BGR555 bits zero through four by consuming presentation loaders.</summary>
    public required int Red { get; init; }
    /// <summary>Green intensity, zero through 31, encoded in BGR555 bits five through nine by consuming presentation loaders.</summary>
    public required int Green { get; init; }
    /// <summary>Blue intensity, zero through 31, encoded in BGR555 bits ten through fourteen by consuming presentation loaders; no alpha channel is represented.</summary>
    public required int Blue { get; init; }
}
/// <summary>Host filename and byte-sized schema limits for editable map highlight palettes.</summary>
public static class MapPaletteCycleFormat
{
    /// <summary>JSON presentation filename used by pause and file-select map highlight animation.</summary>
    public const string FileName = "map-highlight-cycle.json";
    /// <summary>Required cycle-document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of colors written per frame, equal to one sixteen-color native CGRAM row.</summary>
    public const int ColorCount = 16;
    /// <summary>Maximum selected frame count, 255, preserving the animation owner's byte-sized frame cursor.</summary>
    public const int MaximumFrames = 255;
    /// <summary>Maximum authored frame hold, 254 ticks; zero is invalid and the native $FF duration marks stream termination rather than a hold.</summary>
    public const int MaximumDuration = 254;
}
