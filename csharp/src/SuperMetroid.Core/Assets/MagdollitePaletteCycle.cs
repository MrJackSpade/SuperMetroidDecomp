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
    private readonly ushort[][] frames;

    private MagdollitePaletteCycle(ushort[][] frames) => this.frames = frames;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(int frame, int color)
    {
        if ((uint)frame >= frames.Length)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= frames[frame].Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames[frame][color];
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
