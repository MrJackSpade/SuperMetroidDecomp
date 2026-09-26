using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain death-fade and exploded-door RGB5 images.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    private readonly ushort[][] bodyFade;
    private readonly ushort[][] legFade;
    private readonly ushort[][] corpseFade;
    private readonly ushort[] explodedDoor;

    private MotherBrainDeathColorCatalog(ushort[][] bodyFade, ushort[][] legFade,
        ushort[][] corpseFade, ushort[] explodedDoor)
    {
        this.bodyFade = bodyFade;
        this.legFade = legFade;
        this.corpseFade = corpseFade;
        this.explodedDoor = explodedDoor;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort BodyColor(int frame, int color) =>
        Resolve(bodyFade, frame, color, nameof(BodyColor));

    public ushort LegColor(int frame, int color) =>
        Resolve(legFade, frame, color, nameof(LegColor));

    public ushort CorpseColor(int frame, int color) =>
        Resolve(corpseFade, frame, color, nameof(CorpseColor));

    public ushort ExplodedDoorColor(int color) =>
        (uint)color < explodedDoor.Length ? explodedDoor[color] :
            throw new ArgumentOutOfRangeException(nameof(color));

    public static MotherBrainDeathColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        MotherBrainDeathColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<MotherBrainDeathColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Mother Brain death color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Mother Brain death color JSON.", error);
        }
        if (document.Version != MotherBrainDeathColorFormat.Version)
            throw new InvalidDataException(
                "Mother Brain death colors require the supported version.");
        return new(CompileFrames(document.BodyFade,
                MotherBrainDeathRomData.BodyFadeFrameCount,
                MotherBrainDeathRomData.BodyColorCount, "body fade"),
            CompileFrames(document.LegFade,
                MotherBrainDeathRomData.BodyFadeFrameCount,
                MotherBrainDeathRomData.BodyColorCount, "leg fade"),
            CompileFrames(document.CorpseFade,
                MotherBrainDeathRomData.CorpseFadeFrameCount,
                MotherBrainDeathRomData.CorpseColorCount, "corpse fade"),
            Compile(document.ExplodedDoor, MotherBrainDeathRomData.BodyColorCount,
                "exploded door"));
    }

    public static byte[] Write(MotherBrainDeathColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Resolve(ushort[][] frames, int frame, int color, string name) =>
        (uint)frame < frames.Length && (uint)color < frames[frame].Length
            ? frames[frame][color]
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Mother Brain death {name} frame {frame}, color {color} is outside the authored images.");

    private static ushort[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {count} RGB5 colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Mother Brain death {name} color {color} requires RGB5 channels 0..31.");
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
                        $"Duplicate Mother Brain death color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record MotherBrainDeathColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] BodyFade { get; init; }
    public required PaletteRgb5[][] LegFade { get; init; }
    public required PaletteRgb5[][] CorpseFade { get; init; }
    public required PaletteRgb5[] ExplodedDoor { get; init; }
}

public static class MotherBrainDeathColorFormat
{
    public const string FileName = "mother-brain-death-colors.json";
    public const int Version = 1;
}
