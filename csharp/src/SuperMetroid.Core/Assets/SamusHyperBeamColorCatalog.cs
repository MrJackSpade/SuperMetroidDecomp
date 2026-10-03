using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable ten-frame full-body Hyper Beam RGB5 palette cycle.</summary>
/// <remarks>The four distinct color-zero payloads ($3800,$7FFF,$0000,$0400)
/// are retained as unused RGB input, not a hue curve. Native91DD64 copies each
/// row's first word to SpriteP4C0; both playback views preserve that copy.
/// SnesObjRenderer discards index-zero pixels before reading CGRAM, so their
/// RGB bits have no visible color meaning. A phase-to-RGB formula or reciting
/// switch would only re-encode arbitrary unused payloads. Repeated zero rows
/// already share one input; independent edits remain exact. This disposition
/// covers only transparent payloads, not remaining opaque-channel work.</remarks>
public sealed class SamusHyperBeamColorCatalog
{
    private readonly Dictionary<int, ushort> colors = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> intermediateInputs = new();
    private readonly Dictionary<int, EndpointChannels> endpointInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    private SamusHyperBeamColorCatalog(ushort[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        {
            int index = frame * 16 + color, source = SamusHyperBeamColorFormat.CanonicalColorIndex(frame, color);
            if (source != index && frames[frame][color] == frames[source / 16][source % 16]) continue;
            var shade = SamusHyperBeamColorFormat.ShadeSource(color);
            if (source == index && shade.Ink != color)
            {
                if (SamusHyperBeamColorFormat.TryBrighten(frames[frame][shade.Ink], shade.Brightness, out ushort brighter))
                {
                    // Magenta blue follows the supplied red channel, including a red override.
                    ushort expected = frame == 1 ? (ushort)((brighter & 0x03ff) | (frames[frame][color] & 31) << 10) : brighter;
                    if (frames[frame][color] != expected) shadeInputs.Add(index, new(frames[frame][color], expected));
                    continue;
                }
                colors.Add(index, frames[frame][color]);
                continue;
            }
            if (source == index && frame == 7 && color != 0 &&
                frames[frame][color] == SamusHyperBeamColorFormat.YellowFromGreen(frames[5][color])) continue;
            if (source == index && color != 0 && (frame & 1) == 0)
            {
                ushort expected = frame == 6 ?
                    SamusHyperBeamColorFormat.GreenYellowMidpoint(frames[5][color]) :
                    SamusHyperBeamColorFormat.HueMidpoint(frames[(frame + 9) % 10][color], frames[frame + 1][color]);
                if (frames[frame][color] != expected)
                    intermediateInputs.Add(index, new(frames[frame][color], expected));
                continue;
            }
            if (source == index && color != 0 && frame is 1 or 5 or 9)
            {
                endpointInputs.Add(index, new(frames[frame][color], frame == 9,
                    frames[5][color] >> 5 & 31, frame == 1 ? frames[5][color] & 31 : null));
                continue;
            }
            colors.Add(index, frames[frame][color]);
        }
    }

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
        if (document.Version != SamusHyperBeamColorFormat.Version)
            throw new InvalidDataException("Samus Hyper Beam colors require the supported version.");
        return FromFrames(document.Frames);
    }

    /// <summary>Validates independently supplied rows for either native view of the Hyper Beam palette.</summary>
    internal static SamusHyperBeamColorCatalog FromFrames(PaletteRgb5[][]? frames)
    {
        if (frames is null || frames.Length != SamusHyperBeamColorFormat.FrameCount)
            throw new InvalidDataException("Samus Hyper Beam colors require ten frames.");
        var compiled = new ushort[frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = frames[frame];
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
        int source = SamusHyperBeamColorFormat.CanonicalColorIndex(frame, color);
        if (colors.TryGetValue(frame * 16 + color, out ushort value)) return value;
        if (source != frame * 16 + color) return Resolve(source / 16, source % 16);
        var shade = SamusHyperBeamColorFormat.ShadeSource(color);
        if (shade.Ink != color)
        {
            if (SamusHyperBeamColorFormat.TryBrighten(Resolve(frame, shade.Ink), shade.Brightness, out ushort brighter))
            {
                shadeInputs.TryGetValue(frame * 16 + color, out var shadeInput);
                ushort result = shadeInput.Apply(brighter);
                return frame == 1 ? shadeInput.Apply(SamusHyperBeamColorFormat.MagentaFromRed(result)) : result;
            }
            throw new InvalidOperationException("Validated Hyper Beam shade exceeds RGB5.");
        }
        if (endpointInputs.TryGetValue(frame * 16 + color, out var endpoint))
            return endpoint.Resolve(frame == 9, frame == 9 ? Resolve(5, color) >> 5 & 31 : 0,
                frame == 1 ? Resolve(5, color) & 31 : 0);
        if (frame == 7) return SamusHyperBeamColorFormat.YellowFromGreen(Resolve(5, color));
        ushort expected = frame == 6 ? SamusHyperBeamColorFormat.GreenYellowMidpoint(Resolve(5, color)) :
            SamusHyperBeamColorFormat.HueMidpoint(Resolve((frame + 9) % 10, color), Resolve(frame + 1, color));
        return intermediateInputs.TryGetValue(frame * 16 + color, out var inputs) ? inputs.Apply(expected) : expected;
    }

    /// <summary>Independent endpoint channels, sharing equal channels and the green-to-red maximum.</summary>
    /// <remarks>Original rows9B:A340/A2C0 (cycle frames1/5) have blue=red
    /// for every opaque ink. RowA240 (frame9) has blue=green and red=frame5
    /// green. Frame1 green equals frame5 red: the two hues share their minimum.
    /// Keep the distinct endpoint inputs; independent edits override
    /// either relationship. No rounding or saturation occurs. Derivation of
    /// the remaining endpoint shade inputs is still under review in1165.</remarks>
    internal readonly struct EndpointChannels
    {
        private readonly int? red;
        private readonly int? green;
        private readonly int? blue;

        internal EndpointChannels(ushort supplied, bool redHue, int greenHueMaximum, int? greenHueMinimum = null)
        {
            int suppliedRed = supplied & 31;
            int suppliedGreen = supplied >> 5 & 31;
            green = suppliedGreen == greenHueMinimum ? null : suppliedGreen;
            red = redHue && suppliedRed == greenHueMaximum ? null : suppliedRed;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied >> 10 & 31) == expectedBlue ? null : supplied >> 10 & 31;
        }

        internal ushort Resolve(bool redHue, int greenHueMaximum, int greenHueMinimum = 0)
        {
            int resolvedRed = red ?? greenHueMaximum;
            int resolvedGreen = green ?? greenHueMinimum;
            return (ushort)(resolvedRed | resolvedGreen << 5 | (blue ?? (redHue ? resolvedGreen : resolvedRed)) << 10);
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

    /// <summary>Selects the shared shadow ink and constant RGB brightening for an opaque sprite ink.</summary>
    /// <remarks>Across all ten native rows9B:A240..A37F, inks1/8 share ink11
    /// at +2/+4 in each channel; inks2/10/14 share ink3 at +8/+4/+2;
    /// inks4/5/9 share ink13 at +8/+2/+4. These are fixed shade assignments,
    /// independent of hue phase. Ink7 likewise uses ink3+7, with differing
    /// components in frames1/4/8 kept as independent inputs pending review.
    /// Other inks keep their own input.</remarks>
    internal static (int Ink, int Brightness) ShadeSource(int ink) => ink switch
    {
        1 => (11, 2), 8 => (11, 4),
        2 => (3, 8), 7 => (3, 7), 10 => (3, 4), 14 => (3, 2),
        4 => (13, 8), 5 => (13, 2), 9 => (13, 4),
        _ => (ink, 0),
    };

    /// <summary>Adds a supported shade increment independently to all three RGB5 channels.</summary>
    /// <remarks>Original shades never overflow. A supplied edited source that
    /// would exceed31 cannot replace its independently supplied target; import
    /// keeps that target explicit instead of clamping or wrapping a channel.</remarks>
    internal static bool TryBrighten(ushort source, int brightness, out ushort value)
    {
        if (brightness is not (2 or 4 or 7 or 8)) throw new ArgumentOutOfRangeException(nameof(brightness));
        int red = (source & 31) + brightness;
        int green = (source >> 5 & 31) + brightness;
        int blue = (source >> 10 & 31) + brightness;
        if (red > 31 || green > 31 || blue > 31) { value = 0; return false; }
        value = (ushort)(red | green << 5 | blue << 10);
        return true;
    }
    /// <summary>Turns the red endpoint into magenta by raising blue to red.</summary>
    /// <remarks>Original projectile frame8 ink1 ($8D:D9A8) derives from
    /// frame0 ink3 ($D90C). Body-cycle frame1 also has blue=red,including
    /// its independently edited shade-red channels. Red/green stay fixed; copying red into blue
    /// needs no rounding,saturation or overflow. Edits remain independent.</remarks>
    internal static ushort MagentaFromRed(ushort red) =>
        (ushort)((red & 0x03ff) | (red & 31) << 10);
    /// <summary>Interpolates two RGB5 hue endpoints by half, rounding each channel upward.</summary>
    /// <remarks>Even frames interpolate the adjacent odd hue endpoints, with
    /// frame0 wrapping between9/1. Frame6 uses the green-to-yellow red ramp.
    /// Only independently supplied channels differing from this calculation
    /// are stored. Their derivation/disposition remains under review in1165;
    /// matching channels are always calculated, never stored as generated words.
    /// Each independent channel numerator is0..63; no saturation or overflow.</remarks>
    internal static ushort HueMidpoint(ushort first, ushort second)
    {
        int red = ((first & 31) + (second & 31) + 1) / 2;
        int green = ((first >> 5 & 31) + (second >> 5 & 31) + 1) / 2;
        int blue = ((first >> 10 & 31) + (second >> 10 & 31) + 1) / 2;
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Interpolates red halfway from green-frame red to its green value, rounding upward.</summary>
    /// <remarks>Original frame6 ($9B:A2A0) preserves frame5 green/blue;
    /// nine canonical opaque inks also have red=ceil((red5+green5)/2).
    /// Slots1/8/11 supply differing red components pending further review;
    /// their matching green/blue channels are calculated. The numerator is at most63; no saturation or
    /// overflow is needed. This reuses the green-frame input, not a generated cache.</remarks>
    internal static ushort GreenYellowMidpoint(ushort green) =>
        (ushort)((green & 0x7fe0) | ((green & 31) + (green >> 5 & 31) + 1) / 2);

    /// <summary>Changes the green Hyper Beam hue into yellow by raising red to green.</summary>
    /// <remarks>Every opaque original frame7 word ($9B:A280) equals frame5
    /// ($9B:A2C0) with red replaced by green. Green/blue remain unchanged.
    /// RGB5 component copying needs no rounding or saturation. Transparent
    /// payloads are outside the hue transform; differing asset values override it.</remarks>
    internal static ushort YellowFromGreen(ushort green) =>
        (ushort)((green & 0x7fe0) | (green >> 5 & 31));

    /// <summary>Shares repeated Hyper Beam sprite inks and transparent payloads.</summary>
    /// <remarks>Original ten rows selected by91D99E have slots6=2,15=3,
    ///12=10. Transparent frames4..9 equal frame2. These cases preserve original
    /// input ownership with differing asset values stored as overrides. All other
    /// color/shade relationships still require independent review.</remarks>
    internal static int CanonicalColorIndex(int frame, int color)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= ColorsPerFrame) throw new ArgumentOutOfRangeException(nameof(color));
        if (color == 0 && frame >= 4) return 2 * 16;
        int ink = color switch { 6 => 2, 15 => 3, 12 => 10, _ => color };
        return frame * 16 + ink;
    }
}
