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
            Span<ushort> selectedSidehopper = stackalloc ushort[ShitroidColorRomData.TargetColorCount];
            for (int color = 0; color < selectedSidehopper.Length; color++) selectedSidehopper[color] = sidehopper.Resolve(color);
            content.AppendWords("sidehopper", selectedSidehopper);
            Span<ushort> selectedShitroid = stackalloc ushort[ShitroidColorRomData.TargetColorCount];
            for (int color = 0; color < selectedShitroid.Length; color++) selectedShitroid[color] = TargetColor(ShitroidColorTarget.Shitroid, color);
            content.AppendWords("shitroid", selectedShitroid);
            Span<ushort> selectedCorpse = stackalloc ushort[ShitroidColorRomData.TargetColorCount];
            for (int color = 0; color < selectedCorpse.Length; color++) selectedCorpse[color] = deadSidehopper.Resolve(color);
            content.AppendWords("deadSidehopper", selectedCorpse);
            content.Append("normal", ShitroidColorRomData.NormalFrameCount);
            for (int frame = 0; frame < ShitroidColorRomData.NormalFrameCount; frame++)
            {
                var row = new ushort[ShitroidColorRomData.NormalColorsPerFrame];
                for (int color = 0; color < row.Length; color++) row[color] = NormalColor(frame, color);
                content.AppendWords("row", row);
            }
        });

    private readonly ColorPulse normal;
    private readonly SidehopperInitialPalette sidehopper;
    private readonly BabyMetroidInitialPalette shitroid;
    /// <summary>$A9:F8E6 is copied by EF9F-EFA8 to target slot A0. Stock3800 is an exact
    /// transparent-slot compatibility payload: OBJ ink0 is skipped before color lookup.
    /// It has no visible hue to derive; independently supplied replacements remain exact.</summary>
    private readonly ushort shitroidTransparentSlot;
    private readonly SidehopperCorpsePalette deadSidehopper;

    private ShitroidColorCatalog(ushort[][] normal, ushort[] sidehopper,
        ushort[] shitroid, ushort[] deadSidehopper)
    {
        this.shitroid = new BabyMetroidInitialPalette(shitroid.AsSpan(1).ToArray());
        this.normal = new ColorPulse(normal, this.shitroid);
        this.sidehopper = new(sidehopper);
        shitroidTransparentSlot = shitroid[0];
        this.deadSidehopper = new(deadSidehopper);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Gets a packed RGB5 ink from normal pulse phase 0–7 and ink 0–3, corresponding to $A9:F6D1 and written to CGRAM colors 165–168; phase cadence and cries remain actor behavior.</summary>
    public ushort NormalColor(int frame, int color) =>
        (uint)frame < ShitroidColorRomData.NormalFrameCount && (uint)color < ShitroidColorRomData.NormalColorsPerFrame
            ? normal.Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Shitroid normal frame {frame}, color {color} is outside the authored image.");

    /// <summary>Gets packed RGB5 color 0–15 from a selected initialization target, including its retained transparent-slot word; the actor copies targets to its palette image and current CGRAM during setup.</summary>
    public ushort TargetColor(ShitroidColorTarget target, int color)
    {
        if (target == ShitroidColorTarget.Shitroid)
        {
            if ((uint)color >= ShitroidColorRomData.TargetColorCount) throw new ArgumentOutOfRangeException(nameof(color));
            return color == 0 ? shitroidTransparentSlot : shitroid.Resolve(color - 1);
        }
        if (target == ShitroidColorTarget.Sidehopper) return sidehopper.Resolve(color);
        return target == ShitroidColorTarget.DeadSidehopper ? deadSidehopper.Resolve(color) :
            throw new ArgumentOutOfRangeException(nameof(target));
    }

    /// <summary>Loads <c>shitroid-colors.json</c>, requiring version 1, eight four-color pulse frames, three sixteen-color targets, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled colors retaining independently edited frames and targets exactly, sharing the calculated stock pulse only when all samples agree.</returns>
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

    /// <summary>Serializes the editable color document as indented camel-case UTF-8 JSON and validates the serialized schema, dimensions, and channel bounds before returning it.</summary>
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
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Shitroid color property {name}."));
}

/// <summary>Mutually exclusive sixteen-color initialization images for the live giant Baby Metroid encounter and its Sidehopper victim.</summary>
public enum ShitroidColorTarget
{
    /// <summary>$A9:F8C6, live Sidehopper victim target copied to palette colors $90–$9F during encounter initialization.</summary>
    Sidehopper,
    /// <summary>$A9:F8E6, giant Baby Metroid target copied to palette colors $A0–$AF; the first word is the retained transparent-slot payload.</summary>
    Shitroid,
    /// <summary>$A9:F8A6, drained Sidehopper corpse target copied to palette colors $F0–$FF for the encounter's victim transition.</summary>
    DeadSidehopper,
}

/// <summary>Versioned editable RGB5 pulse and initialization-target images for the live Shitroid encounter, excluding timer, fade-destination, and sound behavior.</summary>
public sealed record ShitroidColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Eight ordered phases of four innard inks each, corresponding to $A9:F6D1; all RGB5 channels must be from 0 through 31.</summary>
    public required PaletteRgb5[][] Normal { get; init; }
    /// <summary>Sixteen ordered live-victim colors corresponding to $A9:F8C6, including transparent palette slot zero.</summary>
    public required PaletteRgb5[] Sidehopper { get; init; }
    /// <summary>Sixteen ordered giant Baby Metroid colors corresponding to $A9:F8E6; slot zero is preserved as data even though OBJ transparency gives it no visible hue.</summary>
    public required PaletteRgb5[] Shitroid { get; init; }
    /// <summary>Sixteen ordered drained-victim colors corresponding to $A9:F8A6, including transparent palette slot zero.</summary>
    public required PaletteRgb5[] DeadSidehopper { get; init; }
}

/// <summary>Installed filename and supported schema revision for the encounter's selected pulse and target colors.</summary>
public static class ShitroidColorFormat
{
    /// <summary>Installed editable JSON filename for the eight-frame pulse and three initialization target images.</summary>
    public const string FileName = "shitroid-colors.json";
    /// <summary>Supported color schema revision, requiring the exact native frame and target dimensions.</summary>
    public const int Version = 1;
}
