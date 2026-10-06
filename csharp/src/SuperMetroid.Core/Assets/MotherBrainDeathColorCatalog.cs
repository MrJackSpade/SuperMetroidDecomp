using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain death-fade and exploded-door RGB5 images.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
        {
            Span<ushort> door = stackalloc ushort[MotherBrainExplodedDoorPaintDefinitions.ColorCount];
            for (int color = 0; color < door.Length; color++) door[color] = ExplodedDoorColor(color);
            content.AppendWords("explodedDoor", door);
            bodyFade.AppendIdentity(content, "bodyFade");
            legFade.AppendIdentity(content, "legFade");
            corpseFade.AppendIdentity(content, "corpseFade");
        });

    private readonly ColorFade bodyFade;
    private readonly ColorFade legFade;
    private readonly ColorFade corpseFade;
    private readonly ushort[]? explodedDoor;

    private MotherBrainDeathColorCatalog(ushort[][] bodyFade, ushort[][] legFade,
        ushort[][] corpseFade, ushort[] explodedDoor)
    {
        this.bodyFade = new(bodyFade, toBlack: true);
        this.legFade = new(legFade, toBlack: true, backLeg: true);
        this.corpseFade = new(corpseFade, toBlack: false);
        this.explodedDoor = MotherBrainExplodedDoorPaintDefinitions.Matches(explodedDoor) ? null : explodedDoor;
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
        (uint)color < MotherBrainExplodedDoorPaintDefinitions.ColorCount
            ? explodedDoor is null ? MotherBrainExplodedDoorPaintDefinitions.Color(color) : explodedDoor[color] :
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

    private static ushort Resolve(ColorFade frames, int frame, int color, string name) =>
        (uint)frame < frames.FrameCount && (uint)color < frames.ColorCount
            ? frames.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Mother Brain death {name} frame {frame}, color {color} is outside the authored images.");

    /// <summary>
    /// $AD:EA0A body/leg channels fade to black as (initial*(15-frame)+1)/15.
    /// $AD:F119 corpse channels interpolate between endpoints, rounded to nearest
    /// over seven intervals. These rules match every original intermediate color.
    /// Starting colors calculate from health state three; the corpse final palette calculates shared
    /// drained shade rules from the three narrowly approved drained-paint anchors.
    /// Unmatched edited endpoints and frames remain exact and independent of the health document.
    /// </summary>
    private sealed class ColorFade
    {
        private readonly ushort[]? first;
        private readonly bool backLeg;
        private readonly MotherBrainRainbowPalettePresentation.DrainedBodyColors? last;
        private readonly ushort[][]? supplied;
        internal int FrameCount { get; }
        internal int ColorCount { get; }

        internal ColorFade(ushort[][] frames, bool toBlack, bool backLeg = false)
        {
            this.backLeg = backLeg;
            ColorCount = frames[0].Length;
            for (int color = 0; color < ColorCount; color++)
                if (frames[0][color] != MotherBrainHealthPalettePresentation.StockDeathStartColor(backLeg, color))
                { first = frames[0]; break; }
            last = toBlack ? null : new MotherBrainRainbowPalettePresentation.DrainedBodyColors(frames[^1]);
            FrameCount = frames.Length;
            for (int frame = 0; frame < FrameCount; frame++)
            for (int color = 0; color < ColorCount; color++)
            {
                if (frames[frame][color] != Calculate(frame, color))
                {
                    supplied = frames;
                    return;
                }
            }
        }

        internal ushort Resolve(int frame, int color) =>
            supplied is null ? Calculate(frame, color) : supplied[frame][color];

        private ushort Calculate(int frame, int color)
        {
            int steps = FrameCount - 1;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int start = ((first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(backLeg, color) : first[color]) >> shift) & 31;
                int end = last is null ? 0 : (last[color] >> shift) & 31;
                int bias = last is null ? 1 : steps / 2;
                int channel = (start * (steps - frame) + end * frame + bias) / steps;
                result |= channel << shift;
            }
            return (ushort)result;
        }

        internal void AppendIdentity(SelectedPresentationHash content, string label)
        {
            content.Append(label, FrameCount);
            Span<ushort> row = stackalloc ushort[ColorCount];
            for (int frame = 0; frame < FrameCount; frame++)
            {
                for (int color = 0; color < ColorCount; color++)
                    row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        }
    }

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
