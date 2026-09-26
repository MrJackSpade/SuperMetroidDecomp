using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable default, speed, and shine RGB5 images for Dachora. Native speed/shine
/// timers and their frame-index arithmetic remain compiled enemy behavior.
/// </summary>
public sealed class DachoraColorCatalog
{
    private readonly ushort[] normal;
    private readonly ushort[][] speed;
    private readonly ushort[][] shine;

    private DachoraColorCatalog(ushort[] normal, ushort[][] speed, ushort[][] shine)
    {
        this.normal = normal;
        this.speed = speed;
        this.shine = shine;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(DachoraPalettePhase phase, int frame, int color)
    {
        ushort[] palette = phase switch
        {
            DachoraPalettePhase.Default when frame == 0 => normal,
            DachoraPalettePhase.Speed when (uint)frame < speed.Length => speed[frame],
            DachoraPalettePhase.Shine when (uint)frame < shine.Length => shine[frame],
            _ => throw new ArgumentOutOfRangeException(nameof(frame),
                $"Dachora phase {phase} has no frame {frame}."),
        };
        return (uint)color < palette.Length
            ? palette[color]
            : throw new ArgumentOutOfRangeException(nameof(color));
    }

    public static DachoraColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        DachoraColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<DachoraColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Dachora color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Dachora color JSON.", error);
        }
        if (document.Version != DachoraColorFormat.Version)
            throw new InvalidDataException("Dachora colors require the supported version.");
        return new(Compile(document.Normal, "normal"),
            CompileFrames(document.Speed, "speed"),
            CompileFrames(document.Shine, "shine"));
    }

    public static byte[] Write(DachoraColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[][] CompileFrames(PaletteRgb5[][]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.AnimatedFrameCount)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.AnimatedFrameCount} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != DachoraColorRomData.ColorsPerFrame)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorRomData.ColorsPerFrame} RGB5 colors.");
        var compiled = new ushort[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Dachora {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
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
                        $"Duplicate Dachora color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record DachoraColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Normal { get; init; }
    public required PaletteRgb5[][] Speed { get; init; }
    public required PaletteRgb5[][] Shine { get; init; }
}

public static class DachoraColorFormat
{
    public const string FileName = "dachora-colors.json";
    public const int Version = 1;
}
