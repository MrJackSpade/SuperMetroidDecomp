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
            Span<Bgr555> row = stackalloc Bgr555[WorkRobotPaletteRomData.ColorCount];
            for (int frame = 0; frame < WorkRobotPaletteTimingDefinitions.RecordCount; frame++)
            {
                for (int color = 0; color < row.Length; color++)
                    row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        });

    private readonly Bgr555[][]? frames;

    private WorkRobotPaletteCycle(Bgr555[][] frames)
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
    private static Bgr555 StockColor(int frame, int color)
    {
        int phase = Math.Min(frame, 6 - frame);
        int position = (color + phase) & 3;
        return new Bgr555(31 - 16 * (position >> 1) - 7 * (position & 1), 0, 0);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Resolves one packed RGB555 color from an authored Work Robot cycle frame.</summary>
    /// <param name="frame">Zero-based native palette record from 0 through 5.</param>
    /// <param name="color">Zero-based color within the four-color record.</param>
    /// <returns>The selected packed SNES RGB555 word.</returns>
    public Bgr555 Resolve(int frame, int color)
    {
        if ((uint)frame >= WorkRobotPaletteTimingDefinitions.RecordCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= WorkRobotPaletteRomData.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames is null ? StockColor(frame, color) : frames[frame][color];
    }

    /// <summary>Copies one four-color frame into CGRAM without advancing the native timer.</summary>
    /// <param name="cgram">Destination color memory.</param>
    /// <param name="frame">Zero-based cycle record from 0 through 5.</param>
    /// <param name="destination">First CGRAM color index receiving the record.</param>
    public void ApplyFrame(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destination < 0 ||
            destination + WorkRobotPaletteRomData.ColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < WorkRobotPaletteRomData.ColorCount; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    /// <summary>Loads and validates the six four-color Work Robot frames from JSON.</summary>
    /// <param name="json">Caller-owned stream containing the palette-cycle document.</param>
    /// <returns>The compiled palette cycle.</returns>
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

        var compiled = new Bgr555[WorkRobotPaletteTimingDefinitions.RecordCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != WorkRobotPaletteRomData.ColorCount)
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
    public static byte[] Write(WorkRobotPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Work Robot palette property {name}."));
}

/// <summary>JSON schema for the six ordered Work Robot RGB5 palette records.</summary>
public sealed record WorkRobotPaletteCycleDocument
{
    /// <summary>Gets the schema version required by <see cref="WorkRobotPaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets six frames of four editable RGB5 colors each.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Defines the installed Work Robot palette-cycle resource contract.</summary>
public static class WorkRobotPaletteCycleFormat
{
    /// <summary>Canonical Work Robot palette-cycle asset file name.</summary>
    public const string FileName = "work-robot-palette-cycle.json";
    /// <summary>Supported Work Robot palette-cycle schema version.</summary>
    public const int Version = 1;
}
