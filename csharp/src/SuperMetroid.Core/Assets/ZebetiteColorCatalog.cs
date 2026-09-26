using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable RGB5 artwork for Zebetite's eight-frame two-color pulse.</summary>
public sealed class ZebetiteColorCatalog
{
    private readonly ushort[][] frames;

    private ZebetiteColorCatalog(ushort[][] frames) => this.frames = frames;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static ZebetiteColorCatalog Load(Stream json)
    {
        ZebetiteColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ZebetiteColorDocument>(Options)
                ?? throw new InvalidDataException("Zebetite colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Zebetite color JSON.", error);
        }
        if (document.Version != ZebetiteColorFormat.Version || document.Frames is null ||
            document.Frames.Length != ZebetiteColorFormat.FrameCount)
            throw new InvalidDataException("Zebetite colors require all eight frames at the supported version.");

        var compiled = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (colors is null || colors.Length != ZebetiteColorFormat.ColorsPerFrame)
                throw new InvalidDataException($"Zebetite frame {frame} requires two RGB5 colors.");
            compiled[frame] = new ushort[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Zebetite frame {frame}, color {color} requires RGB5 channels.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new(compiled);
    }

    public void Apply(SnesCgram cgram, int frame, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            cgram.SetColor(destinationColor + color, frames[frame][color]);
    }

    public static byte[] Write(ZebetiteColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
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
                    throw new InvalidDataException("Duplicate Zebetite color property.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record ZebetiteColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Presentation geometry of Zebetite palette records at $A6:FD87..FDA6.</summary>
public static class ZebetiteColorFormat
{
    public const string FileName = "zebetite-colors.json";
    public const int Version = 1;
    public const int FrameCount = 8;
    public const int ColorsPerFrame = 2;
}
