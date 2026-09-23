using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable ten-frame full-body Hyper Beam RGB5 palette cycle.</summary>
public sealed class SamusHyperBeamColorCatalog
{
    private readonly ushort[][] frames;

    private SamusHyperBeamColorCatalog(ushort[][] frames) => this.frames = frames;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static SamusHyperBeamColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusHyperBeamColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusHyperBeamColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus Hyper Beam color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus Hyper Beam color JSON.", error);
        }
        if (document.Version != SamusHyperBeamColorFormat.Version ||
            document.Frames is null || document.Frames.Length != SamusHyperBeamColorFormat.FrameCount)
            throw new InvalidDataException("Samus Hyper Beam colors require the supported version and ten frames.");
        var compiled = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != SamusHyperBeamColorFormat.ColorsPerFrame)
                throw new InvalidDataException($"Samus Hyper Beam frame {frame} requires sixteen RGB5 colors.");
            compiled[frame] = new ushort[source.Length];
            for (int colorIndex = 0; colorIndex < source.Length; colorIndex++)
            {
                PaletteRgb5? color = source[colorIndex];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Samus Hyper Beam frame {frame} color {colorIndex} requires RGB components from zero through 31.");
                compiled[frame][colorIndex] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            }
        }
        return new(compiled);
    }

    public static byte[] Write(SamusHyperBeamColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Returns display color only; the frame clock remains cartridge-owned.</summary>
    public ushort Resolve(int frame, int color)
    {
        if ((uint)frame >= frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= frames[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
        return frames[frame][color];
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Samus Hyper Beam color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record SamusHyperBeamColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

public static class SamusHyperBeamColorFormat
{
    public const string FileName = "samus-hyper-beam-colors.json";
    public const int Version = 1;
    public const int FrameCount = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount;
    public const int ColorsPerFrame = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
