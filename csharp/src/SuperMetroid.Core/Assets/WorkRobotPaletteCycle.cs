using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Work Robot RGB5 frames. Native record order, 64/16-tick durations,
/// and OBJ palette ownership remain engine-defined.
/// </summary>
public sealed class WorkRobotPaletteCycle
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1", content =>
        {
            content.Append("frames", WorkRobotPaletteTimingDefinitions.RecordCount);
            Span<ushort> row = stackalloc ushort[WorkRobotPaletteRomData.ColorCount];
            for (int frame = 0; frame < WorkRobotPaletteTimingDefinitions.RecordCount; frame++)
            {
                for (int color = 0; color < row.Length; color++)
                    row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        });

    private readonly ushort[][]? frames;

    private WorkRobotPaletteCycle(ushort[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        {
            if (frames[frame][color] != StockColor(frame, color))
            {
                this.frames = frames;
                return;
            }
        }
    }

    /// <summary>
    /// $A8:CCC1, six four-color Work Robot records: red-only brightness rotates
    /// forward three positions and back. The four brightness levels form two
    /// pairs separated by sixteen, with seven between the members of each pair.
    /// </summary>
    private static ushort StockColor(int frame, int color)
    {
        int phase = Math.Min(frame, 6 - frame);
        int position = (color + phase) & 3;
        return (ushort)(31 - 16 * (position >> 1) - 7 * (position & 1));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(int frame, int color)
    {
        if ((uint)frame >= WorkRobotPaletteTimingDefinitions.RecordCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= WorkRobotPaletteRomData.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames is null ? StockColor(frame, color) : frames[frame][color];
    }

    public void ApplyFrame(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destination < 0 ||
            destination + WorkRobotPaletteRomData.ColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < WorkRobotPaletteRomData.ColorCount; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    public static WorkRobotPaletteCycle Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        WorkRobotPaletteCycleDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<WorkRobotPaletteCycleDocument>(JsonOptions)
                ?? throw new InvalidDataException("Work Robot palette cycle JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Work Robot palette cycle JSON.", error);
        }

        if (document.Version != WorkRobotPaletteCycleFormat.Version ||
            document.Frames is null ||
            document.Frames.Length != WorkRobotPaletteTimingDefinitions.RecordCount)
            throw new InvalidDataException("Work Robot palette cycle requires six version-one frames.");

        var compiled = new ushort[WorkRobotPaletteTimingDefinitions.RecordCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != WorkRobotPaletteRomData.ColorCount)
                throw new InvalidDataException($"Work Robot palette frame {frame} requires four RGB5 colors.");
            compiled[frame] = new ushort[source.Length];
            for (int color = 0; color < source.Length; color++)
            {
                PaletteRgb5? rgb = source[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Work Robot palette frame {frame} color {color} requires RGB5 channels 0..31.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new WorkRobotPaletteCycle(compiled);
    }

    public static byte[] Write(WorkRobotPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate Work Robot palette property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record WorkRobotPaletteCycleDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

public static class WorkRobotPaletteCycleFormat
{
    public const string FileName = "work-robot-palette-cycle.json";
    public const int Version = 1;
}
