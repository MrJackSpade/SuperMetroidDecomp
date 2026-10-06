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
            Span<ushort> selectedShitroid = stackalloc ushort[ShitroidColorRomData.TargetColorCount];
            for (int color = 0; color < selectedShitroid.Length; color++) selectedShitroid[color] = TargetColor(ShitroidColorTarget.Shitroid, color);
            content.AppendWords("shitroid", selectedShitroid);
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
    private readonly BabyMetroidInitialPalette shitroid;
    /// <summary>$A9:F8E6 is copied by EF9F-EFA8 to target slot A0. Stock3800 is an exact
    /// transparent-slot compatibility payload: OBJ ink0 is skipped before color lookup.
    /// It has no visible hue to derive; independently supplied replacements remain exact.</summary>
    private readonly ushort shitroidTransparentSlot;
    private readonly ushort[] deadSidehopper;

    private ShitroidColorCatalog(ushort[][] normal, ushort[] sidehopper,
        ushort[] shitroid, ushort[] deadSidehopper)
    {
        this.shitroid = new BabyMetroidInitialPalette(shitroid.AsSpan(1).ToArray());
        this.normal = new ColorPulse(normal, this.shitroid);
        this.sidehopper = sidehopper;
        shitroidTransparentSlot = shitroid[0];
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
        if (target == ShitroidColorTarget.Shitroid)
        {
            if ((uint)color >= ShitroidColorRomData.TargetColorCount) throw new ArgumentOutOfRangeException(nameof(color));
            return color == 0 ? shitroidTransparentSlot : shitroid.Resolve(color - 1);
        }
        ushort[] selected = target switch
        {
            ShitroidColorTarget.Sidehopper => sidehopper,
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
        private readonly BabyMetroidInitialPalette target;
        private readonly ushort[][]? supplied;

        public ColorPulse(ushort[][] frames, BabyMetroidInitialPalette target)
        {
            this.target = target;
            // Sharing the target's innard colors is valid only when every supplied
            // pulse sample agrees. Independently edited targets or frames stay exact.
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < ShitroidColorRomData.NormalColorsPerFrame; color++)
                if (Resolve(frame, color) != frames[frame][color])
                { supplied = frames; return; }
        }

        public ushort Resolve(int frame, int color)
        {
            if (supplied is not null) return supplied[frame][color];
            int phase = Math.Min(frame, ShitroidColorRomData.NormalFrameCount - 1 - frame);
            ushort paint = target.Resolve(ShitroidColorRomData.NormalFirstInitialColor + color);
            int result = 0;
            for (int channel = 0; channel < 3; channel++)
            {
                int origin = paint >> (5 * channel) & 31;
                int floor = 0;
                if (channel == 0)
                {
                    floor = ShitroidColorRomData.NormalOrganRedFloor;
                    if (origin == 31) origin += ShitroidColorRomData.NormalOrganRedHeadroom;
                }
                result |= Math.Clamp(origin - ShitroidColorRomData.NormalOrganDimmingStep * phase,
                    floor, 31) << (5 * channel);
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
