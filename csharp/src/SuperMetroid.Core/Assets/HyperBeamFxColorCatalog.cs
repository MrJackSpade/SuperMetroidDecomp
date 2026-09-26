using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Authored colors for the ten Hyper Beam projectile-palette frames, not their timing or control program.</summary>
public sealed class HyperBeamFxColorCatalog
{
    private readonly ushort[][] frames;

    private HyperBeamFxColorCatalog(ushort[][] frames) => this.frames = frames;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static HyperBeamFxColorCatalog Load(Stream json)
    {
        HyperBeamFxColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<HyperBeamFxColorDocument>(Options)
                ?? throw new InvalidDataException("Hyper Beam FX colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Hyper Beam FX color JSON.", error);
        }

        if (document.Version != HyperBeamFxColorFormat.Version ||
            document.Frames is null || document.Frames.Length != HyperBeamFxColorFormat.FrameCount)
            throw new InvalidDataException("Hyper Beam FX colors require ten frames at the supported version.");

        var compiled = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (colors is null || colors.Length != HyperBeamFxColorFormat.ColorsPerFrame)
                throw new InvalidDataException($"Hyper Beam FX frame {frame} requires eight colors.");
            compiled[frame] = new ushort[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Hyper Beam FX frame {frame}, color {color} requires RGB5 components.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new(compiled);
    }

    public void Apply(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
            cgram.SetColor(destination + color, frames[frame][color]);
    }

    public static byte[] Write(HyperBeamFxColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicateProperties(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException("Duplicate Hyper Beam FX color property.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicateProperties(child);
    }
}

public sealed record HyperBeamFxColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Presentation geometry of the color payloads at $8D:D906..D9CA.</summary>
public static class HyperBeamFxColorFormat
{
    public const string FileName = "hyper-beam-fx-colors.json";
    public const int Version = 1;
    public const int FrameCount = 10;
    public const int ColorsPerFrame = 8;
}
