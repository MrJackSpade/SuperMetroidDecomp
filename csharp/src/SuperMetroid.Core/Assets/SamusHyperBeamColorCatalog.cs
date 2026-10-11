using SuperMetroid.Core.Hardware;
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

    private readonly Dictionary<int, Bgr555> colors = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> intermediateInputs = new();
    private readonly Dictionary<int, EndpointChannels> endpointInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    private SamusHyperBeamColorCatalog(Bgr555[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        {
            int index = frame * 16 + color, source = SamusHyperBeamColorFormat.CanonicalColorIndex(frame, color);
            if (source != index && frames[frame][color] == frames[source / 16][source % 16]) continue;
            var shade = SamusHyperBeamColorFormat.ShadeSource(color);
            if (source == index && shade.Ink != color)
            {
                if (SamusHyperBeamColorFormat.TryBrighten(frames[frame][shade.Ink], shade.Brightness, out Bgr555 brighter))
                {
                    // Magenta blue follows the supplied red channel, including a red override.
                    Bgr555 expected = frame == 1 ? brighter.WithBlue(frames[frame][color].Red) : brighter;
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
                if (SamusHyperBeamColorFormat.TryGreenYellowHighShadow(frames[6][3], frames[6][13], frames[5][11], out Bgr555 expected))
                {
                    if (frames[frame][color] != expected) intermediateInputs.Add(index, new(frames[frame][color], expected));
                }
                else colors.Add(index, frames[frame][color]);
                continue;
            }
            if (frame == 2 && color is 3 or 11 or 13)
            {
                Bgr555 expected;
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
                Bgr555 expected = frame == 6 ?
                    SamusHyperBeamColorFormat.GreenYellowMidpoint(frames[5][color]) :
                    SamusHyperBeamColorFormat.HueMidpoint(frames[(frame + 9) % 10][color], frames[frame + 1][color]);
                if (SamusHyperBeamColorFormat.SharedIntermediateShadowChannel(frame, color) is ColorChannel shared)
                    expected = expected.With(shared, frames[frame][13][shared]);
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
                Bgr555 expected = SamusHyperBeamColorFormat.HueMidpoint(frames[3][3], frames[3][11]);
                if (frames[frame][color] != expected)
                    intermediateInputs.Add(index, new(frames[frame][color], expected));
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
        if (!SamusHyperBeamColorFormat.Shape.HasRows(frames))
            throw new InvalidDataException("Samus Hyper Beam colors require ten frames.");
        var compiled = new Bgr555[frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = frames[frame];
            if (!SamusHyperBeamColorFormat.Shape.HasColumns(source))
                throw new InvalidDataException($"Samus Hyper Beam frame {frame} requires sixteen RGB5 colors.");
            compiled[frame] = new Bgr555[source.Length];
            for (int colorIndex = 0; colorIndex < source.Length; colorIndex++)
            {
                PaletteRgb5? color = source[colorIndex];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Samus Hyper Beam frame {frame} color {colorIndex} requires RGB components from zero through 31.");
                compiled[frame][colorIndex] = color.ToBgr555();
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
    public Bgr555 Resolve(int frame, int color)
    {
        int source = SamusHyperBeamColorFormat.CanonicalColorIndex(frame, color);
        if (colors.TryGetValue(frame * 16 + color, out Bgr555 value)) return value;
        if (source != frame * 16 + color) return Resolve(source / 16, source % 16);
        var shade = SamusHyperBeamColorFormat.ShadeSource(color);
        if (shade.Ink != color)
        {
            if (SamusHyperBeamColorFormat.TryBrighten(Resolve(frame, shade.Ink), shade.Brightness, out Bgr555 brighter))
            {
                shadeInputs.TryGetValue(frame * 16 + color, out var shadeInput);
                Bgr555 result = shadeInput.Apply(brighter);
                return frame == 1 ? shadeInput.Apply(SamusHyperBeamColorFormat.MagentaFromRed(result)) : result;
            }
            throw new InvalidOperationException("Validated Hyper Beam shade exceeds RGB5.");
        }
        if (endpointInputs.TryGetValue(frame * 16 + color, out var endpoint))
        {
            var basis = SamusHyperBeamColorFormat.EndpointSourceChannels(frame, color,
                frame == 5 ? Bgr555.Black : Resolve(5, color),
                color == 13 ? Resolve(frame, 3) : Bgr555.Black,
                color == 13 ? Resolve(frame, 11) : Bgr555.Black);
            return endpoint.Resolve(frame == 9, basis.Red ?? 0, basis.Green ?? 0);
        }
        if (frame == 2 && color is 3 or 11 or 13)
        {
            Bgr555 transition;
            if (color == 13) transition = SamusHyperBeamColorFormat.CyanTransitionMiddle(Resolve(1, 13), Resolve(3, 13));
            else if (!SamusHyperBeamColorFormat.TryCyanTransitionShadow(Resolve(2, 13), color, out transition))
                throw new InvalidOperationException("Validated Hyper Beam transition shadow exceeds RGB5.");
            return intermediateInputs.TryGetValue(frame * 16 + color, out var transitionInput) ? transitionInput.Apply(transition) : transition;
        }
        if (frame == 3 && color == 13)
        {
            Bgr555 middle = SamusHyperBeamColorFormat.HueMidpoint(Resolve(3, 3), Resolve(3, 11));
            return intermediateInputs.TryGetValue(frame * 16 + color, out var middleInput) ? middleInput.Apply(middle) : middle;
        }
        if (frame == 6 && color == 11)
        {
            if (!SamusHyperBeamColorFormat.TryGreenYellowHighShadow(Resolve(6, 3), Resolve(6, 13), Resolve(5, 11), out Bgr555 high))
                throw new InvalidOperationException("Validated Hyper Beam shadow exceeds RGB5.");
            return intermediateInputs.TryGetValue(frame * 16 + color, out var highInput) ? highInput.Apply(high) : high;
        }
        if (frame == 7) return SamusHyperBeamColorFormat.YellowFromGreen(Resolve(5, color));
        Bgr555 expected = frame == 6 ? SamusHyperBeamColorFormat.GreenYellowMidpoint(Resolve(5, color)) :
            SamusHyperBeamColorFormat.HueMidpoint(Resolve((frame + 9) % 10, color), Resolve(frame + 1, color));
        if (SamusHyperBeamColorFormat.SharedIntermediateShadowChannel(frame, color) is ColorChannel shared)
            expected = expected.With(shared, Resolve(frame, 13)[shared]);
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
        private readonly int? red;
        private readonly int? green;
        private readonly int? blue;

        internal EndpointChannels(Bgr555 supplied, bool redHue, int? expectedRed, int? expectedGreen = null)
        {
            int suppliedRed = supplied.Red;
            int suppliedGreen = supplied.Green;
            green = suppliedGreen == expectedGreen ? null : suppliedGreen;
            red = suppliedRed == expectedRed ? null : suppliedRed;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied.Blue) == expectedBlue ? null : supplied.Blue;
        }

        internal Bgr555 Resolve(bool redHue, int expectedRed, int expectedGreen = 0)
        {
            int resolvedRed = red ?? expectedRed;
            int resolvedGreen = green ?? expectedGreen;
            return new(resolvedRed, resolvedGreen, blue ?? (redHue ? resolvedGreen : resolvedRed));
        }
    }
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
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);

    /// <summary>Selects a shadow channel shared with the same frame's middle ink13.</summary>
    /// <remarks>Native frame0 ink3 red and ink11 green at9BA366/A376
    /// share middle ink13 red21/green3. Frame8 ink11 blue at9BA276 shares
    /// middle blue3. Other channels keep their temporal blend. Return the
    /// RGB5 bit shift or-1 when no within-frame sharing applies.</remarks>
    internal static ColorChannel? SharedIntermediateShadowChannel(int frame, int ink) => (frame, ink) switch
    {
        (0, 3) => ColorChannel.Red,
        (0, 11) => ColorChannel.Green,
        (8, 11) => ColorChannel.Blue,
        _ => null,
    };

    /// <summary>Interpolates the magenta-to-cyan middle shadow, rounding toward cyan.</summary>
    /// <remarks>Native frame2 ink13 at9BA33A is RGB10/7/18 between
    /// magenta19/0/19 and cyan2/13/17. Floor the red half-sum and ceil the
    /// green/blue half-sums: the quantization favors the cyan channels.
    /// No fixed correction value is added. Numerators0..63,RGB5 inputs only.
    /// This states the exact color construction,not the original tool's identity.</remarks>
    internal static Bgr555 CyanTransitionMiddle(Bgr555 magenta, Bgr555 cyan)
    {
        return HueMidpoint(magenta, cyan).WithRed(HueMidpoint(magenta, cyan, roundUp: false).Red);
    }

    /// <summary>Builds the lower and upper shadows around the magenta-cyan middle ink.</summary>
    /// <remarks>Original frame2 middle13 is10/7/18. Lower3 is the same
    /// red/blue with green one step lower; upper11 is one RGB step brighter.
    /// This gives10/6/18 and11/8/19 without independent intermediate inputs.
    /// Edited middle colors may exceed those operations' RGB5 bounds; return
    /// false so import preserves the supplied whole target,without saturation.</remarks>
    internal static bool TryCyanTransitionShadow(Bgr555 middle, int ink, out Bgr555 value)
    {
        if (ink is not (3 or 11)) throw new ArgumentOutOfRangeException(nameof(ink));
        int red = middle.Red, green = middle.Green, blue = middle.Blue;
        if (ink == 3)
        {
            if (green == 0) { value = Bgr555.Black; return false; }
            value = middle.WithGreen(green - 1);
        }
        else
        {
            if (red == 31 || green == 31 || blue == 31) { value = Bgr555.Black; return false; }
            value = new(red + 1, green + 1, blue + 1);
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
        Bgr555 greenHue, Bgr555 lowShadow, Bgr555 highShadow)
    {
        if (frame is not (1 or 5 or 9)) throw new ArgumentOutOfRangeException(nameof(frame));
        if (color is not (3 or 11 or 13)) throw new ArgumentOutOfRangeException(nameof(color));
        int? red = frame == 9 ? greenHue.Green : null;
        int? green = frame == 1 ? greenHue.Red : null;
        if (color == 13)
        {
            Bgr555 middle = HueMidpoint(lowShadow, highShadow);
            if (frame == 1) red = middle.Red;
            if (frame == 5) red = lowShadow.Red;
            if (frame is 5 or 9) green = middle.Green;
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
    internal static bool TryBrighten(Bgr555 source, int brightness, out Bgr555 value)
    {
        if (brightness is not (2 or 4 or 7 or 8)) throw new ArgumentOutOfRangeException(nameof(brightness));
        int red = (source.Red) + brightness;
        int green = (source.Green) + brightness;
        int blue = (source.Blue) + brightness;
        if (red > 31 || green > 31 || blue > 31) { value = Bgr555.Black; return false; }
        value = new Bgr555(red, green, blue);
        return true;
    }
    /// <summary>Turns the red endpoint into magenta by raising blue to red.</summary>
    /// <remarks>Original projectile frame8 ink1 ($8D:D9A8) derives from
    /// frame0 ink3 ($D90C). Body-cycle frame1 also has blue=red,including
    /// its independently edited shade-red channels. Red/green stay fixed; copying red into blue
    /// needs no rounding,saturation or overflow. Edits remain independent.</remarks>
    internal static Bgr555 MagentaFromRed(Bgr555 red) =>
        red.WithBlue(red.Red);
    /// <summary>Interpolates RGB5 colors by half with the caller's channel rounding convention.</summary>
    /// <remarks>Even frames interpolate the adjacent odd hue endpoints, with
    /// frame0 wrapping between9/1. Frame6 uses the green-to-yellow red ramp.
    /// Only independently edited channels differing from the selected color
    /// construction are stored; original intermediate colors need no corrections.
    /// matching channels are always calculated, never stored as generated words.
    /// Upward rounding is the default for hue blends. The magenta-cyan
    /// middle shadow selects downward red and upward green/blue rounding.
    /// Each independent channel numerator is0..63; no saturation or overflow.</remarks>
    internal static Bgr555 HueMidpoint(Bgr555 first, Bgr555 second, bool roundUp = true)
    {
        int rounding = roundUp ? 1 : 0;
        int red = ((first.Red) + (second.Red) + rounding) / 2;
        int green = ((first.Green) + (second.Green) + rounding) / 2;
        int blue = ((first.Blue) + (second.Blue) + rounding) / 2;
        return new Bgr555(red, green, blue);
    }

    /// <summary>Interpolates red halfway from green-frame red to its green value, rounding upward.</summary>
    /// <remarks>Original frame6 ($9B:A2A0) preserves frame5 green/blue;
    /// nine canonical opaque inks also have red=ceil((red5+green5)/2).
    /// Ink11 instead follows the within-frame red shadow ramp,with inks1/8
    /// deriving their brighter shades from it. Green/blue remain shared.
    /// The numerator is at most63; no saturation or
    /// overflow is needed. This reuses the green-frame input, not a generated cache.</remarks>
    internal static Bgr555 GreenYellowMidpoint(Bgr555 green) =>
        green.WithRed((green.Red + green.Green + 1) / 2);

    /// <summary>Extends the green-yellow red shadow ramp by its existing equal step.</summary>
    /// <remarks>Native frame6 inks3/13/11 at9BA2A6/A2BA/A2B6 have red11/12/13.
    /// Calculate the upper red as2*middle-low; green23/blue1 remain those of
    /// green frame5 ink11. This preserves the spatial shade ramp instead of
    /// storing a red correction to the temporal midpoint. RGB5 inputs only.
    /// Edited shadow endpoints may yield red-31..62; return false outside0..31
    /// so import keeps the independently supplied whole target,without clamping.</remarks>
    internal static bool TryGreenYellowHighShadow(Bgr555 low, Bgr555 middle, Bgr555 green, out Bgr555 value)
    {
        int red = 2 * (middle.Red) - (low.Red);
        if ((uint)red > 31) { value = Bgr555.Black; return false; }
        value = green.WithRed(red);
        return true;
    }

    /// <summary>Changes the green Hyper Beam hue into yellow by raising red to green.</summary>
    /// <remarks>Every opaque original frame7 word ($9B:A280) equals frame5
    /// ($9B:A2C0) with red replaced by green. Green/blue remain unchanged.
    /// RGB5 component copying needs no rounding or saturation. Transparent
    /// payloads are outside the hue transform; differing asset values override it.</remarks>
    internal static Bgr555 YellowFromGreen(Bgr555 green) =>
        green.WithRed(green.Green);

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
