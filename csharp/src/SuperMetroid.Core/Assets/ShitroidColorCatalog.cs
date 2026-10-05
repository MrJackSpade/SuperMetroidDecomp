using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable live-Shitroid RGB5 images; native timer and fade destinations stay compiled.</summary>
public sealed class ShitroidColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
        {
            content.AppendWords("sidehopper", sidehopper);
            content.AppendWords("shitroid", shitroid);
            content.AppendWords("deadSidehopper", deadSidehopper);
            content.Append("normal", ShitroidColorRomData.NormalFrameCount);
            for (int frame = 0; frame < ShitroidColorRomData.NormalFrameCount; frame++)
            {
                var row = new ushort[ShitroidColorRomData.NormalColorsPerFrame];
                for (int color = 0; color < row.Length; color++) row[color] = NormalColor(frame, color);
                content.AppendWords("row", row);
            }
        });

    private readonly ColorPulse normal;
    private readonly ushort[] sidehopper;
    private readonly ushort[] shitroid;
    private readonly ushort[] deadSidehopper;

    private ShitroidColorCatalog(ushort[][] normal, ushort[] sidehopper,
        ushort[] shitroid, ushort[] deadSidehopper)
    {
        this.normal = new ColorPulse(normal);
        this.sidehopper = sidehopper;
        this.shitroid = shitroid;
        this.deadSidehopper = deadSidehopper;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort NormalColor(int frame, int color) =>
        (uint)frame < ShitroidColorRomData.NormalFrameCount && (uint)color < ShitroidColorRomData.NormalColorsPerFrame
            ? normal.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Shitroid normal frame {frame}, color {color} is outside the authored image.");

    public ushort TargetColor(ShitroidColorTarget target, int color)
    {
        ushort[] selected = target switch
        {
            ShitroidColorTarget.Sidehopper => sidehopper,
            ShitroidColorTarget.Shitroid => shitroid,
            ShitroidColorTarget.DeadSidehopper => deadSidehopper,
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };
        return (uint)color < selected.Length ? selected[color] :
            throw new ArgumentOutOfRangeException(nameof(color));
    }

    public static ShitroidColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ShitroidColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ShitroidColorDocument>(JsonOptions) ??
                throw new InvalidDataException("Shitroid color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Shitroid color JSON.", error);
        }
        if (document.Version != ShitroidColorFormat.Version)
            throw new InvalidDataException("Shitroid colors require the supported version.");
        if (document.Normal is null ||
            document.Normal.Length != ShitroidColorRomData.NormalFrameCount)
            throw new InvalidDataException("Shitroid normal cycle requires eight frames.");
        return new(document.Normal.Select((frame, index) =>
                Compile(frame, ShitroidColorRomData.NormalColorsPerFrame,
                    $"normal frame {index}")).ToArray(),
            Compile(document.Sidehopper, ShitroidColorRomData.TargetColorCount, "sidehopper"),
            Compile(document.Shitroid, ShitroidColorRomData.TargetColorCount, "Shitroid"),
            Compile(document.DeadSidehopper, ShitroidColorRomData.TargetColorCount,
                "dead sidehopper"));
    }

    public static byte[] Write(ShitroidColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int required, string name)
    {
        if (source is null || source.Length != required)
            throw new InvalidDataException($"Shitroid {name} requires {required} RGB5 colors.");
        var compiled = new ushort[required];
        for (int color = 0; color < required; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Shitroid {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private sealed class ColorPulse
    {
        private readonly uint[] origins;
        private readonly ushort minimum;
        private readonly ushort[][]? supplied;

        public ColorPulse(ushort[][] frames)
        {
            origins = new uint[ShitroidColorRomData.NormalColorsPerFrame];
            for (int channel = 0; channel < 3; channel++)
            {
                int floor = 31;
                foreach (ushort[] frame in frames)
                foreach (ushort color in frame)
                    floor = Math.Min(floor, color >> (5 * channel) & 31);
                minimum |= (ushort)(floor << (5 * channel));
            }
            for (int color = 0; color < origins.Length; color++)
            for (int channel = 0; channel < 3; channel++)
            {
                int floor = minimum >> (5 * channel) & 31;
                int origin = floor;
                for (int phase = 0; phase < 4; phase++)
                {
                    int value = frames[phase][color] >> (5 * channel) & 31;
                    if (value > floor) origin = Math.Max(origin, value + 5 * phase);
                }
                origins[color] |= (uint)origin << (8 * channel);
            }
            // Four dimming levels subtract five per RGB channel, then retrace.
            // The red highlights begin above RGB5's ceiling; dark reds have a floor.
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < origins.Length; color++)
                if (Resolve(frame, color) != frames[frame][color])
                { supplied = frames; return; }
        }

        public ushort Resolve(int frame, int color)
        {
            if (supplied is not null) return supplied[frame][color];
            int phase = Math.Min(frame, 7 - frame);
            int result = 0;
            for (int channel = 0; channel < 3; channel++)
            {
                int origin = (int)(origins[color] >> (8 * channel) & 255);
                int floor = minimum >> (5 * channel) & 31;
                result |= Math.Clamp(origin - 5 * phase, floor, 31) << (5 * channel);
            }
            return (ushort)result;
        }
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
                        $"Duplicate Shitroid color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public enum ShitroidColorTarget
{
    Sidehopper,
    Shitroid,
    DeadSidehopper,
}

public sealed record ShitroidColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Normal { get; init; }
    public required PaletteRgb5[] Sidehopper { get; init; }
    public required PaletteRgb5[] Shitroid { get; init; }
    public required PaletteRgb5[] DeadSidehopper { get; init; }
}

public static class ShitroidColorFormat
{
    public const string FileName = "shitroid-colors.json";
    public const int Version = 1;
}
