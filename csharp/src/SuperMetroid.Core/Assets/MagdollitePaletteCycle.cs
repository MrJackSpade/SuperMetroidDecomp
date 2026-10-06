using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable RGB5 colors for Magdollite's four-frame OBJ palette cycle.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("MagdollitePaletteCycle-v1", content =>
        {
            content.Append("frames", MagdollitePaletteRomData.FrameCount);
            Span<ushort> row = stackalloc ushort[MagdollitePaletteRomData.AnimatedColorCount];
            for (int frame = 0; frame < MagdollitePaletteRomData.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        });

    // The four independent glow colors remain required artwork under issue1165.
    private readonly ushort[] colors;
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>
    /// Palette_Magdollite_Glow_0..3 at $A8:AC2E-AC34 rotates left one color per
    /// phase in the four native OBJ rows. Preserve arbitrary edited later rows
    /// as deviations from that rotation, without rebuilding the native table.
    /// </summary>
    private MagdollitePaletteCycle(ushort[][] frames)
    {
        colors = frames[0];
        for (int frame = 1; frame < frames.Length; frame++)
        for (int color = 0; color < colors.Length; color++)
            if (frames[frame][color] != colors[(color + frame) % colors.Length])
                edits.Add(frame * colors.Length + color, frames[frame][color]);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(int frame, int color)
    {
        if ((uint)frame >= MagdollitePaletteRomData.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(frame * colors.Length + color, out ushort edited)
            ? edited : colors[(color + frame) % colors.Length];
    }

    public void ApplyFrame(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destination < 0 ||
            destination + MagdollitePaletteRomData.AnimatedColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    public static MagdollitePaletteCycle Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        MagdollitePaletteCycleDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<MagdollitePaletteCycleDocument>(JsonOptions)
                ?? throw new InvalidDataException("Magdollite palette cycle JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Magdollite palette cycle JSON.", error);
        }

        if (document.Version != MagdollitePaletteCycleFormat.Version ||
            document.Frames is null ||
            document.Frames.Length != MagdollitePaletteRomData.FrameCount)
            throw new InvalidDataException("Magdollite palette cycle requires four version-one frames.");

        var compiled = new ushort[MagdollitePaletteRomData.FrameCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != MagdollitePaletteRomData.AnimatedColorCount)
                throw new InvalidDataException($"Magdollite palette frame {frame} requires four RGB5 colors.");
            compiled[frame] = new ushort[source.Length];
            for (int color = 0; color < source.Length; color++)
            {
                PaletteRgb5? rgb = source[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Magdollite palette frame {frame} color {color} requires RGB5 channels 0..31.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new MagdollitePaletteCycle(compiled);
    }

    public static byte[] Write(MagdollitePaletteCycleDocument document)
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
                        $"Duplicate Magdollite palette property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record MagdollitePaletteCycleDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

public static class MagdollitePaletteCycleFormat
{
    public const string FileName = "magdollite-palette-cycle.json";
    public const int Version = 1;
}
