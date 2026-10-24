using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

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

    /// <summary>Independent RGB5 inputs that cannot be reconstructed from shared hue and shade rules.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> colors = new();
    /// <summary>Per-channel deviations from derived temporal midpoint colors.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> intermediateInputs = new();
    /// <summary>Independent red, green, or blue channels at authored hue endpoints.</summary>
    internal readonly Dictionary<int, EndpointChannels> endpointInputs = new();
    /// <summary>Edited highlight channels that differ from shared ink brightening.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    /// <summary>Separates supplied palette rows into independent artwork inputs and calculated relationships.</summary>
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
                colors.Add(new ColourCell(frame, color), frames[frame][color]);
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
                else colors.Add(new ColourCell(frame, color), frames[frame][color]);
                continue;
            }
            if (frame == 2 && color is 3 or 11 or 13)
            {
                Bgr555 expected;
                if (color == 13) expected = SamusHyperBeamColorFormat.CyanTransitionMiddle(frames[1][13], frames[3][13]);
                else if (!SamusHyperBeamColorFormat.TryCyanTransitionShadow(frames[2][13], color, out expected))
                {
                    colors.Add(new ColourCell(frame, color), frames[frame][color]);
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
            colors.Add(new ColourCell(frame, color), frames[frame][color]);
        }
    }

    /// <summary>Validates and compiles the ten full-body Hyper Beam RGB5 rows, preserving independently supplied color edits while leaving palette-cycle timing to Samus's native handler.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled packed-color catalog detached from the document arrays, with shared hue/shade relationships resolved on demand.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, dimensions other than ten rows of sixteen colors, null colors, or channels outside 0..31.</exception>
    public static SamusHyperBeamColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        SamusHyperBeamColorDocument document;
        document = JsonAssetDocument.Read<SamusHyperBeamColorDocument>(json, ArtworkJson.Options, "Samus Hyper Beam color");
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
    public static byte[] Write(SamusHyperBeamColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

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
        if (colors.TryGetValue(new ColourCell(frame, color), out Bgr555 value)) return value;
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
        /// <summary>Supplied red component only when it differs from the endpoint's shared basis.</summary>
        internal readonly int? red;
        /// <summary>Supplied green component only when it differs from the endpoint's shared basis.</summary>
        internal readonly int? green;
        /// <summary>Supplied blue component only when it differs from the endpoint's channel-copy rule.</summary>
        internal readonly int? blue;

        /// <summary>Retains only independently chosen endpoint channels from a supplied RGB5 word.</summary>
        internal EndpointChannels(Bgr555 supplied, bool redHue, int? expectedRed, int? expectedGreen = null)
        {
            int suppliedRed = supplied.Red;
            int suppliedGreen = supplied.Green;
            green = suppliedGreen == expectedGreen ? null : suppliedGreen;
            red = suppliedRed == expectedRed ? null : suppliedRed;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied.Blue) == expectedBlue ? null : supplied.Blue;
        }

        /// <summary>Rebuilds an endpoint by combining supplied channels with shared hue relationships.</summary>
        internal Bgr555 Resolve(bool redHue, int expectedRed, int expectedGreen = 0)
        {
            int resolvedRed = red ?? expectedRed;
            int resolvedGreen = green ?? expectedGreen;
            return new(resolvedRed, resolvedGreen, blue ?? (redHue ? resolvedGreen : resolvedRed));
        }
    }
}
