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
/// already share one input; independent edits remain exact.
///
/// Fourteen opaque endpoint components are retained as chosen artwork colors:
/// cyan frame3 shadow3/11 RGB; magenta frame1 shadow3/11 red; green frame5
/// shadow3/11 red/green; red frame9 shadow3/11 green. They specify the tint
/// and contrast of the two source shadow inks in each named hue. Native91D96F
/// selects a complete palette,91DD64 copies it,and the renderer uses the
/// painted pixel's ink index. No measured lighting/material quantity supplies
/// those endpoint colors. Shared hue channels,middle shadows,brighter inks and
/// temporal blends are calculated separately. A fitted endpoint-index curve
/// or RGB-reciting cases would only encode the artist's color choices,the
/// specific1165 nonsense exception.
///
/// Three local highlight inputs are likewise selected artwork accents for
/// ink7:frame1 red26 (blue shares red),frame4 blue16,and frame8 green21.
/// These selectively raise that painted ink by8 instead of the calculated7,
/// emphasizing magenta red/blue,cyan-green blue,and orange green respectively.
/// A universal channel-brightness rule cannot account for blue18 becoming26
/// in frame1 but25 in frame2. The palette chooses the accent for that hue;
/// no runtime lighting quantity selects it. Per-phase numeric corrections or
/// fitted coefficients would only recite those particular color-design choices.
/// Retain these three inputs under the specific artwork/nonsense exception.
/// Intermediate colors are calculated from hue blends and shared shadow groups;
/// no original intermediate correction components remain stored.</remarks>
public sealed class SamusHyperBeamColorCatalog
{
    /// <summary>Explicit RGB5 values that remain independent of the catalog's shared hue and shade calculations.</summary>
    private readonly Dictionary<int, ushort> colors = new();
    /// <summary>Per-channel inputs retained where a calculated intermediate blend differs from the supplied artwork.</summary>
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> intermediateInputs = new();
    /// <summary>Independent endpoint channel choices used by the calculated magenta, green, and red rows.</summary>
    private readonly Dictionary<int, EndpointChannels> endpointInputs = new();
    /// <summary>Per-channel highlight accents retained when the shared shade rule does not reproduce the supplied ink.</summary>
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    /// <summary>Compiles supplied palette rows into explicit values and only the independent inputs needed by shared color rules.</summary>
    /// <param name="frames">Ten rows of packed RGB5 colors used to derive and preserve catalog inputs.</param>
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
            if (frame == 6 && color == 11)
            {
                if (SamusHyperBeamColorFormat.TryGreenYellowHighShadow(frames[6][3], frames[6][13], frames[5][11], out ushort expected))
                {
                    if (frames[frame][color] != expected) intermediateInputs.Add(index, new(frames[frame][color], expected));
                }
                else colors.Add(index, frames[frame][color]);
                continue;
            }
            if (frame == 2 && color is 3 or 11 or 13)
            {
                ushort expected;
                if (color == 13) expected = SamusHyperBeamColorFormat.CyanTransitionMiddle(frames[1][13], frames[3][13]);
                else if (!SamusHyperBeamColorFormat.TryCyanTransitionShadow(frames[2][13], color, out expected))
                {
                    colors.Add(index, frames[frame][color]);
                    continue;
                }
                if (frames[frame][color] != expected) intermediateInputs.Add(index, new(frames[frame][color], expected));
                continue;
            }
            if (source == index && color != 0 && (frame & 1) == 0)
            {
                ushort expected = frame == 6 ?
                    SamusHyperBeamColorFormat.GreenYellowMidpoint(frames[5][color]) :
                    SamusHyperBeamColorFormat.HueMidpoint(frames[(frame + 9) % 10][color], frames[frame + 1][color]);
                int shift = SamusHyperBeamColorFormat.SharedIntermediateShadowChannel(frame, color);
                if (shift >= 0) expected = (ushort)((expected & ~(31 << shift)) | (frames[frame][13] & (31 << shift)));
                if (frames[frame][color] != expected)
                    intermediateInputs.Add(index, new(frames[frame][color], expected));
                continue;
            }
            if (source == index && color != 0 && frame is 1 or 5 or 9)
            {
                var basis = SamusHyperBeamColorFormat.EndpointSourceChannels(frame, color,
                    frames[5][color], frames[frame][3], frames[frame][11]);
                endpointInputs.Add(index, new(frames[frame][color], frame == 9,
                    basis.Red, basis.Green));
                continue;
            }
            if (frame == 3 && color == 13)
            {
                ushort expected = SamusHyperBeamColorFormat.HueMidpoint(frames[3][3], frames[3][11]);
                if (frames[frame][color] != expected)
                    intermediateInputs.Add(index, new(frames[frame][color], expected));
                continue;
            }
            colors.Add(index, frames[frame][color]);
        }
    }

    /// <summary>JSON configuration requiring camel-case members, rejecting unknown fields, and formatting saved artwork for review.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Validates and compiles the ten full-body Hyper Beam RGB5 rows, preserving independently supplied color edits while leaving palette-cycle timing to Samus's native handler.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled packed-color catalog detached from the document arrays, with shared hue/shade relationships resolved on demand.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, dimensions other than ten rows of sixteen colors, null colors, or channels outside 0..31.</exception>
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

    /// <summary>Serializes the editable full-body palette cycle to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Ten complete RGB5 palette rows to serialize; the writer does not retain their arrays.</param>
    /// <returns>Validated JSON bytes for <see cref="SamusHyperBeamColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, palette-dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(SamusHyperBeamColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Returns display color only; the frame clock remains cartridge-owned.</summary>
    /// <remarks>Cyan frame3 ink13 at9BA31A is the upward-rounded RGB5
    /// midpoint of shadow inks3/11 at9BA306/A316. Their RGB1/12/16 and
    /// 3/13/18 yield2/13/17. Reuse the same bounded midpoint operation as
    /// the temporal hue blends; independently supplied channels override it.
    /// Magenta-cyan frame2 instead builds its three shadows around the cyan-biased
    /// midpoint of the neighboring middle shadows; see CyanTransitionMiddle.
    /// This converts that shade relationship without retaining a generated word.</remarks>
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
        {
            var basis = SamusHyperBeamColorFormat.EndpointSourceChannels(frame, color,
                frame == 5 ? (ushort)0 : Resolve(5, color),
                color == 13 ? Resolve(frame, 3) : (ushort)0,
                color == 13 ? Resolve(frame, 11) : (ushort)0);
            return endpoint.Resolve(frame == 9, basis.Red ?? 0, basis.Green ?? 0);
        }
        if (frame == 2 && color is 3 or 11 or 13)
        {
            ushort transition;
            if (color == 13) transition = SamusHyperBeamColorFormat.CyanTransitionMiddle(Resolve(1, 13), Resolve(3, 13));
            else if (!SamusHyperBeamColorFormat.TryCyanTransitionShadow(Resolve(2, 13), color, out transition))
                throw new InvalidOperationException("Validated Hyper Beam transition shadow exceeds RGB5.");
            return intermediateInputs.TryGetValue(frame * 16 + color, out var transitionInput) ? transitionInput.Apply(transition) : transition;
        }
        if (frame == 3 && color == 13)
        {
            ushort middle = SamusHyperBeamColorFormat.HueMidpoint(Resolve(3, 3), Resolve(3, 11));
            return intermediateInputs.TryGetValue(frame * 16 + color, out var middleInput) ? middleInput.Apply(middle) : middle;
        }
        if (frame == 6 && color == 11)
        {
            if (!SamusHyperBeamColorFormat.TryGreenYellowHighShadow(Resolve(6, 3), Resolve(6, 13), Resolve(5, 11), out ushort high))
                throw new InvalidOperationException("Validated Hyper Beam shadow exceeds RGB5.");
            return intermediateInputs.TryGetValue(frame * 16 + color, out var highInput) ? highInput.Apply(high) : high;
        }
        if (frame == 7) return SamusHyperBeamColorFormat.YellowFromGreen(Resolve(5, color));
        ushort expected = frame == 6 ? SamusHyperBeamColorFormat.GreenYellowMidpoint(Resolve(5, color)) :
            SamusHyperBeamColorFormat.HueMidpoint(Resolve((frame + 9) % 10, color), Resolve(frame + 1, color));
        int shift = SamusHyperBeamColorFormat.SharedIntermediateShadowChannel(frame, color);
        if (shift >= 0) expected = (ushort)((expected & ~(31 << shift)) | (Resolve(frame, 13) & (31 << shift)));
        return intermediateInputs.TryGetValue(frame * 16 + color, out var inputs) ? inputs.Apply(expected) : expected;
    }

    /// <summary>Independent endpoint channels, sharing equal channels and the green-to-red maximum.</summary>
    /// <remarks>Original rows9B:A340/A2C0 (cycle frames1/5) have blue=red
    /// for every opaque ink. RowA240 (frame9) has blue=green and red=frame5
    /// green. Frame1 green equals frame5 red: the two hues share their minimum.
    /// EndpointSourceChannels also derives middle-shadow inputs. Keep only
    /// independently differing channels; channel copying itself needs no rounding
    /// or saturation. The remaining endpoint shade inputs have the specific
    /// class-level artwork disposition; intermediate relationships are calculated.</remarks>
    internal readonly struct EndpointChannels
    {
        /// <summary>Supplied red component when it differs from the hue-derived endpoint value.</summary>
        private readonly int? red;
        /// <summary>Supplied green component when it differs from the hue-derived endpoint value.</summary>
        private readonly int? green;
        /// <summary>Supplied blue component when it differs from the channel shared by this hue.</summary>
        private readonly int? blue;

        /// <summary>Stores only endpoint components that cannot be recovered from the shared hue channels.</summary>
        /// <param name="supplied">Packed RGB5 endpoint color from the artwork row.</param>
        /// <param name="redHue">Whether blue follows the resolved red channel rather than green.</param>
        /// <param name="expectedRed">Red component produced by the endpoint relationship, if defined.</param>
        /// <param name="expectedGreen">Green component produced by the endpoint relationship, if defined.</param>
        internal EndpointChannels(ushort supplied, bool redHue, int? expectedRed, int? expectedGreen = null)
        {
            int suppliedRed = supplied & 31;
            int suppliedGreen = supplied >> 5 & 31;
            green = suppliedGreen == expectedGreen ? null : suppliedGreen;
            red = suppliedRed == expectedRed ? null : suppliedRed;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied >> 10 & 31) == expectedBlue ? null : supplied >> 10 & 31;
        }

        /// <summary>Combines stored endpoint exceptions with the calculated hue components into one RGB5 color.</summary>
        /// <param name="redHue">Whether the default blue channel follows the resolved red channel.</param>
        /// <param name="expectedRed">Calculated red component used when no independent red input was stored.</param>
        /// <param name="expectedGreen">Calculated green component used when no independent green input was stored.</param>
        /// <returns>The packed RGB5 endpoint color.</returns>
        internal ushort Resolve(bool redHue, int expectedRed, int expectedGreen = 0)
        {
            int resolvedRed = red ?? expectedRed;
            int resolvedGreen = green ?? expectedGreen;
            return (ushort)(resolvedRed | resolvedGreen << 5 | (blue ?? (redHue ? resolvedGreen : resolvedRed)) << 10);
        }
    }
    /// <summary>Rejects repeated object property names before the JSON document is deserialized.</summary>
    /// <param name="value">Root JSON element to validate for duplicate properties.</param>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus Hyper Beam color property {name}."));
}

/// <summary>Editable ten-row full-body Hyper Beam palette schema, separate from the gameplay-owned cycle clock and acquisition state.</summary>
public sealed record SamusHyperBeamColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusHyperBeamColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of sixteen nonnull RGB5 colors, corresponding to $91:D99E's descending sources $9B:A360..A240; includes color zero's copied payload even though OBJ index-zero pixels remain transparent.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Installed-resource identity and native row geometry for the full-body Hyper Beam RGB5 cycle.</summary>
public static class SamusHyperBeamColorFormat
{
    /// <summary>JSON resource filename containing the ten editable full-body Hyper Beam palettes.</summary>
    public const string FileName = "samus-hyper-beam-colors.json";
    /// <summary>Supported schema revision, one, requiring all ten complete RGB5 palette rows.</summary>
    public const int Version = 1;
    /// <summary>Ten authored palette phases selected by $91:D99E; this count does not specify the number of gameplay updates between phases.</summary>
    public const int FrameCount = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount;
    /// <summary>Sixteen colors in each complete OBJ palette, including transparent index zero and fifteen opaque ink slots.</summary>
    public const int ColorsPerFrame = SamusPaletteRomData.Common.ColorsPerObjPalette;

    /// <summary>Selects a shadow channel shared with the same frame's middle ink13.</summary>
    /// <remarks>Native frame0 ink3 red and ink11 green at9BA366/A376
    /// share middle ink13 red21/green3. Frame8 ink11 blue at9BA276 shares
    /// middle blue3. Other channels keep their temporal blend. Return the
    /// RGB5 bit shift or-1 when no within-frame sharing applies.</remarks>
    internal static int SharedIntermediateShadowChannel(int frame, int ink) => (frame, ink) switch
    {
        (0, 3) => 0,
        (0, 11) => 5,
        (8, 11) => 10,
        _ => -1,
    };

    /// <summary>Interpolates the magenta-to-cyan middle shadow, rounding toward cyan.</summary>
    /// <remarks>Native frame2 ink13 at9BA33A is RGB10/7/18 between
    /// magenta19/0/19 and cyan2/13/17. Floor the red half-sum and ceil the
    /// green/blue half-sums: the quantization favors the cyan channels.
    /// No fixed correction value is added. Numerators0..63,RGB5 inputs only.
    /// This states the exact color construction,not the original tool's identity.</remarks>
    internal static ushort CyanTransitionMiddle(ushort magenta, ushort cyan)
    {
        if (magenta > 0x7fff || cyan > 0x7fff) throw new ArgumentOutOfRangeException(nameof(magenta));
        return (ushort)((HueMidpoint(magenta, cyan, roundUp: false) & 31) |
            (HueMidpoint(magenta, cyan) & 0x7fe0));
    }

    /// <summary>Builds the lower and upper shadows around the magenta-cyan middle ink.</summary>
    /// <remarks>Original frame2 middle13 is10/7/18. Lower3 is the same
    /// red/blue with green one step lower; upper11 is one RGB step brighter.
    /// This gives10/6/18 and11/8/19 without independent intermediate inputs.
    /// Edited middle colors may exceed those operations' RGB5 bounds; return
    /// false so import preserves the supplied whole target,without saturation.</remarks>
    internal static bool TryCyanTransitionShadow(ushort middle, int ink, out ushort value)
    {
        if (middle > 0x7fff) throw new ArgumentOutOfRangeException(nameof(middle));
        if (ink is not (3 or 11)) throw new ArgumentOutOfRangeException(nameof(ink));
        int red = middle & 31, green = middle >> 5 & 31, blue = middle >> 10;
        if (ink == 3)
        {
            if (green == 0) { value = 0; return false; }
            value = (ushort)(middle - 32);
        }
        else
        {
            if (red == 31 || green == 31 || blue == 31) { value = 0; return false; }
            value = (ushort)(middle + 0x421);
        }
        return true;
    }

    /// <summary>Derives shared hue channels and the middle shadow's endpoint components.</summary>
    /// <remarks>Native endpoint frames1/5/9 are magenta,green,red at
    /// 9BA340/A2C0/A240. Their shadow ink13 uses the midpoint of inks3/11:
    /// magenta red18/20 gives19; green green22/23 rounds up to23;
    /// red green4/6 gives5. Green ink13 shares ink3's red/blue minimum0.
    /// Existing hue rules share magenta green with green-hue red,and red-hue
    /// red with green-hue green. Blue derives from the appropriate resolved
    /// red/green channel. Null components remain independently supplied.
    /// Domain:frames1/5/9,shadow inks3/11/13,RGB5 inputs. The reused midpoint
    /// is bounded,no clamping or generated color storage is needed.</remarks>
    internal static (int? Red, int? Green) EndpointSourceChannels(int frame, int color,
        ushort greenHue, ushort lowShadow, ushort highShadow)
    {
        if (frame is not (1 or 5 or 9)) throw new ArgumentOutOfRangeException(nameof(frame));
        if (color is not (3 or 11 or 13)) throw new ArgumentOutOfRangeException(nameof(color));
        if (greenHue > 0x7fff || lowShadow > 0x7fff || highShadow > 0x7fff)
            throw new ArgumentOutOfRangeException(nameof(greenHue));
        int? red = frame == 9 ? greenHue >> 5 & 31 : null;
        int? green = frame == 1 ? greenHue & 31 : null;
        if (color == 13)
        {
            ushort middle = HueMidpoint(lowShadow, highShadow);
            if (frame == 1) red = middle & 31;
            if (frame == 5) red = lowShadow & 31;
            if (frame is 5 or 9) green = middle >> 5 & 31;
        }
        return (red, green);
    }

    /// <summary>Selects the shared shadow ink and constant RGB brightening for an opaque sprite ink.</summary>
    /// <remarks>Across all ten native rows9B:A240..A37F, inks1/8 share ink11
    /// at +2/+4 in each channel; inks2/10/14 share ink3 at +8/+4/+2;
    /// inks4/5/9 share ink13 at +8/+2/+4. These are fixed shade assignments,
    /// independent of hue phase. Ink7 likewise uses ink3+7, with differing
    /// components in frames1/4/8 kept as independently selected highlight
    /// accents under the catalog's class-level artwork disposition.
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
    /// <summary>Interpolates RGB5 colors by half with the caller's channel rounding convention.</summary>
    /// <remarks>Even frames interpolate the adjacent odd hue endpoints, with
    /// frame0 wrapping between9/1. Frame6 uses the green-to-yellow red ramp.
    /// Only independently edited channels differing from the selected color
    /// construction are stored; original intermediate colors need no corrections.
    /// matching channels are always calculated, never stored as generated words.
    /// Upward rounding is the default for hue blends. The magenta-cyan
    /// middle shadow selects downward red and upward green/blue rounding.
    /// Each independent channel numerator is0..63; no saturation or overflow.</remarks>
    internal static ushort HueMidpoint(ushort first, ushort second, bool roundUp = true)
    {
        int rounding = roundUp ? 1 : 0;
        int red = ((first & 31) + (second & 31) + rounding) / 2;
        int green = ((first >> 5 & 31) + (second >> 5 & 31) + rounding) / 2;
        int blue = ((first >> 10 & 31) + (second >> 10 & 31) + rounding) / 2;
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Interpolates red halfway from green-frame red to its green value, rounding upward.</summary>
    /// <remarks>Original frame6 ($9B:A2A0) preserves frame5 green/blue;
    /// nine canonical opaque inks also have red=ceil((red5+green5)/2).
    /// Ink11 instead follows the within-frame red shadow ramp,with inks1/8
    /// deriving their brighter shades from it. Green/blue remain shared.
    /// The numerator is at most63; no saturation or
    /// overflow is needed. This reuses the green-frame input, not a generated cache.</remarks>
    internal static ushort GreenYellowMidpoint(ushort green) =>
        (ushort)((green & 0x7fe0) | ((green & 31) + (green >> 5 & 31) + 1) / 2);

    /// <summary>Extends the green-yellow red shadow ramp by its existing equal step.</summary>
    /// <remarks>Native frame6 inks3/13/11 at9BA2A6/A2BA/A2B6 have red11/12/13.
    /// Calculate the upper red as2*middle-low; green23/blue1 remain those of
    /// green frame5 ink11. This preserves the spatial shade ramp instead of
    /// storing a red correction to the temporal midpoint. RGB5 inputs only.
    /// Edited shadow endpoints may yield red-31..62; return false outside0..31
    /// so import keeps the independently supplied whole target,without clamping.</remarks>
    internal static bool TryGreenYellowHighShadow(ushort low, ushort middle, ushort green, out ushort value)
    {
        if (low > 0x7fff || middle > 0x7fff || green > 0x7fff) throw new ArgumentOutOfRangeException(nameof(low));
        int red = 2 * (middle & 31) - (low & 31);
        if ((uint)red > 31) { value = 0; return false; }
        value = (ushort)((green & 0x7fe0) | red);
        return true;
    }

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
