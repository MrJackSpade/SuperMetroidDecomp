using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Hyper Beam projectile hues with shared white and calculated intermediate frames.</summary>
/// <remarks>The remaining34 independent RGB5 components are color-design
/// inputs under1165's nonsense exception,not an unresolved numeric lookup.
/// Native8D:D900 selects OBJ palette6 colors1..8; the handler copies the eight
/// chosen ink colors. The pixel index selects a painted ink,not a measured
/// light level,hue angle or material parameter. The remaining inputs choose
/// the endpoint tints and local shade emphasis. Replacing them with fitted
/// coefficients or per-ink RGB cases would merely encode the same artwork.
///
/// The retained inputs are: one neutral intensity; paired components
/// (frame,ink,channel)0/3/G,4/4/R,4/6/R,8/3/R,8/4/G,8/6/G,8/7/R;
/// middle-shade components2/2/RG,4/2/GB,6/2/B,6/5/G; endpoint components
/// 0/1/GB,2/1/RG,2/3/RG,4/1/B,4/3/GB,6/1/G,6/3/GB,6/4/RG;
/// and all channels of blue-frame inks6/7. All other original components
/// derive through the documented hue,shade,neutral and shared-channel rules.
/// Asset edits remain independent; this disposition does not cover the
/// separate full-body Hyper Beam palette or palette-program controls.</remarks>
public sealed class HyperBeamFxColorCatalog
{
    private readonly Dictionary<int, Bgr555> colors = new();
    private readonly int neutralIntensity;
    private readonly LoadingPaletteInputView.Channels neutralOverrides;
    private readonly Dictionary<int, PairedChannels> pairedInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> endpointInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    private HyperBeamFxColorCatalog(Bgr555[][] frames)
    {
        neutralIntensity = frames[0][0].Red;
        neutralOverrides = new(frames[0][0], HyperBeamFxColorFormat.Neutral(neutralIntensity));
        for (int frame = 0; frame < HyperBeamFxColorFormat.FrameCount; frame++)
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
        {
            Bgr555 value = frames[frame][color];
            if (frame == 0 && color == 0) continue;
            if (color == 0 && frame != 0 && value == frames[0][0]) continue;
            if (color != 0 && (frame & 1) != 0 && value == SamusHyperBeamColorFormat.HueMidpoint(
                frames[frame - 1][color], frames[(frame + 1) % HyperBeamFxColorFormat.FrameCount][color])) continue;
            if (frame == 2 && color >= 4 && value == SamusHyperBeamColorFormat.YellowFromGreen(frames[4][color])) continue;
            if (frame == 0 && color >= 4 && value == HyperBeamFxColorFormat.RedHighlight(frames[0][3], frames[0][0], color)) continue;
            if (HyperBeamFxColorFormat.IsShadeMidpoint(frame, color))
            {
                Bgr555 expected = SamusHyperBeamColorFormat.HueMidpoint(frames[frame][color - 1], frames[frame][color + 1]);
                if (value != expected) shadeInputs.Add(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, new(value, expected));
                continue;
            }
            if (frame == 4 && color == 7 && value == HyperBeamFxColorFormat.GreenFromRed(frames[0][3])) continue;
            if (frame == 8 && color == 1 && value == SamusHyperBeamColorFormat.MagentaFromRed(frames[0][3])) continue;
            if (HasPairedChannels(frame, color))
            {
                var shared = SharedEndpointChannels(frame, color, frames[0][0], frames[0][3]);
                pairedInputs.Add(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, new(value, frame == 0, shared.Red, shared.Green));
                continue;
            }
            if (HyperBeamFxColorFormat.TrySharedEndpoint(frame, color, frames[0][0], frames[0][3], out Bgr555 basis, out int independentMask))
            {
                endpointInputs.Add(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, new(value, basis, independentMask));
                continue;
            }
            colors.Add(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, value);
        }
    }

    /// <summary>Resolves one validated hue input, repeated white, or adjacent-endpoint midpoint.</summary>
    /// <remarks>Original8D:D906+20*frame contains eight RGB5 words. Color0
    /// repeats frame0 in all ten rows; every other odd-frame color is the
    /// upward-rounded midpoint of the adjacent even frames, wrapping9 to0.
    /// Reuse the independently proved RGB5 midpoint operation. Import stores
    /// differing supplied values as explicit inputs, never generated colors.
    /// Frame2 highlight inks4..7 at8D:D936..D93C preserve frame4 green/blue
    /// and raise red to green: the same green-to-yellow transform used by the
    /// body cycle. Remaining independent inputs have the class-level art disposition.</remarks>
    private Bgr555 Resolve(int frame, int color)
    {
        if (frame == 0 && color == 0) return neutralOverrides.Apply(HyperBeamFxColorFormat.Neutral(neutralIntensity));
        if (colors.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out Bgr555 value)) return value;
        if (pairedInputs.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out var paired))
        {
            var shared = SharedEndpointChannels(frame, color, Resolve(0, 0), frame == 0 ? Bgr555.Black : Resolve(0, 3));
            return paired.Resolve(frame == 0, shared.Red ?? 0, shared.Green ?? 0);
        }
        if (endpointInputs.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out var endpoint))
        {
            HyperBeamFxColorFormat.TrySharedEndpoint(frame, color, Resolve(0, 0), Resolve(0, 3), out Bgr555 basis, out _);
            return endpoint.Apply(basis);
        }
        if (color == 0) return Resolve(0, 0);
        if (frame == 4 && color == 7) return HyperBeamFxColorFormat.GreenFromRed(Resolve(0, 3));
        if (frame == 8 && color == 1) return SamusHyperBeamColorFormat.MagentaFromRed(Resolve(0, 3));
        if (HyperBeamFxColorFormat.IsShadeMidpoint(frame, color))
        {
            Bgr555 expected = SamusHyperBeamColorFormat.HueMidpoint(Resolve(frame, color - 1), Resolve(frame, color + 1));
            return shadeInputs.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out var inputs) ? inputs.Apply(expected) : expected;
        }
        if (frame == 0) return HyperBeamFxColorFormat.RedHighlight(Resolve(0, 3), Resolve(0, 0), color);
        if (frame == 2) return SamusHyperBeamColorFormat.YellowFromGreen(Resolve(4, color));
        return SamusHyperBeamColorFormat.HueMidpoint(
            Resolve(frame - 1, color), Resolve((frame + 1) % HyperBeamFxColorFormat.FrameCount, color));
    }

    /// <summary>Selects the red, green and magenta endpoint inks with two equal channels.</summary>
    /// <remarks>Original red frame0 inks3..7 have blue=green; green frame4
    /// inks4..7 and magenta frame8 inks1..7 have blue=red. Frame addresses
    /// start8D:D906 and advance20 bytes. These are hue-channel equalities;
    /// their independent shade intensities have the class-level art disposition.</remarks>
    private static bool HasPairedChannels(int frame, int color) =>
        frame == 0 && color >= 3 || frame == 4 && color >= 4 || frame == 8 && color != 0;

    /// <summary>Shares hue extrema while leaving the varying shade channel independent.</summary>
    /// <remarks>Original red endpoint uses white's red maximum. Green
    /// highlights use that red maximum as green. Magenta highlight inks use
    /// it as red, while shadow inks3/7 use the red endpoint's green minimum.
    /// All relationships are between supplied RGB5 inputs; differing edits
    /// stay explicit. Independent shade intensities have the class-level art disposition.</remarks>
    private static (int? Red, int? Green) SharedEndpointChannels(int frame, int color, Bgr555 white, Bgr555 red) =>
        frame == 0 ? (white.Red, null) :
        frame == 4 ? (null, red.Red) :
        color is 3 or 7 ? (null, red.Green) : (red.Red, null);
    internal readonly struct PairedChannels
    {
        private readonly int? red;
        private readonly int? green;
        private readonly int? blue;

        internal PairedChannels(Bgr555 supplied, bool redHue, int? sharedRed = null, int? sharedGreen = null)
        {
            int suppliedRed = supplied.Red, suppliedGreen = supplied.Green;
            red = suppliedRed == sharedRed ? null : suppliedRed;
            green = suppliedGreen == sharedGreen ? null : suppliedGreen;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied.Blue) == expectedBlue ? null : supplied.Blue;
        }

        internal Bgr555 Resolve(bool redHue, int sharedRed = 0, int sharedGreen = 0)
        {
            int resolvedRed = red ?? sharedRed, resolvedGreen = green ?? sharedGreen;
            return new(resolvedRed, resolvedGreen, blue ?? (redHue ? resolvedGreen : resolvedRed));
        }
    }
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads ten eight-color projectile palette rows, rejecting duplicate or unknown properties, unsupported versions, incorrect dimensions, and RGB5 channels outside 0..31; preserves supplied edits independently of shared-color and midpoint relationships.</summary>
    /// <param name="json">UTF-8 JSON stream containing the versioned projectile-color document.</param>
    /// <returns>The validated catalog used by the Hyper Beam palette-FX handler.</returns>
    public static HyperBeamFxColorCatalog Load(Stream json)
    {
        HyperBeamFxColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<HyperBeamFxColorDocument>(Options)
                ?? throw new InvalidDataException("Hyper Beam FX colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Hyper Beam FX color JSON.", error);
        }

        if (document.Version != HyperBeamFxColorFormat.Version ||
            document.Frames is null || document.Frames.Length != HyperBeamFxColorFormat.FrameCount)
            throw new InvalidDataException("Hyper Beam FX colors require ten frames at the supported version.");

        var compiled = new Bgr555[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (colors is null || colors.Length != HyperBeamFxColorFormat.ColorsPerFrame)
                throw new InvalidDataException($"Hyper Beam FX frame {frame} requires eight colors.");
            compiled[frame] = new Bgr555[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Hyper Beam FX frame {frame}, color {color} requires RGB5 components.");
                compiled[frame][color] = rgb.ToBgr555();
            }
        }
        return new(compiled);
    }

    /// <summary>Resolves and writes the eight colors of one projectile palette row without advancing animation state; the native Hyper Beam handler targets OBJ palette 6 colors 1..8, CGRAM entries 225..232.</summary>
    /// <param name="cgram">Destination color memory to update.</param>
    /// <param name="frame">Zero-based palette row, from 0 through 9.</param>
    /// <param name="destination">CGRAM color-entry index of the first output color, not a byte offset; the following seven entries are also written.</param>
    public void Apply(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= HyperBeamFxColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    /// <summary>Serializes the projectile-color document as indented camel-case UTF-8 JSON, then validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all ten eight-color RGB5 rows.</param>
    /// <returns>Validated JSON bytes suitable for the projectile-color asset file.</returns>
    public static byte[] Write(HyperBeamFxColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicateProperties(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Hyper Beam FX color property."));
}

/// <summary>Editable JSON schema for the ten Hyper Beam projectile palette rows; the separate native palette program retains frame durations, destination selection, and loop control.</summary>
public sealed record HyperBeamFxColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="HyperBeamFxColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of eight RGB5 colors corresponding to payloads at <c>$8D:D906 + 20 * row</c>; row colors map to OBJ palette 6 colors 1..8, and every supplied channel remains independently editable.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Presentation geometry of the color payloads at $8D:D906..D9CA.</summary>
public static class HyperBeamFxColorFormat
{
    /// <summary>Supplies shared extrema for the remaining red,yellow,green and blue endpoint inks.</summary>
    /// <remarks>Original8D:D906+20*frame: red ink1 shares white red;
    /// yellow inks1/3 share red's minimum as blue; green inks1/3 share that
    /// minimum as red,with ink1 also sharing the red maximum as green.
    /// Blue inks1/3 share the red minimum as red; inks1/4 share white blue.
    /// Other components remain independent inputs,including zero-valued edits.</remarks>
    internal static bool TrySharedEndpoint(int frame, int ink, Bgr555 white, Bgr555 red, out Bgr555 basis, out int independentMask)
    {
        int minimum = red.Green, maximum = red.Red;
        (Bgr555 Value, int Mask) selected = (frame, ink) switch
        {
            (0, 1) => (new Bgr555(white.Red, 0, 0), 6),
            (2, 1 or 3) => (new Bgr555(0, 0, minimum), 3),
            (4, 1) => (new Bgr555(minimum, maximum, 0), 4),
            (4, 3) or (6, 3) => (new Bgr555(minimum, 0, 0), 6),
            (6, 1) => (new Bgr555(minimum, 0, white.Blue), 2),
            (6, 4) => (new Bgr555(0, 0, white.Blue), 3),
            _ => (Bgr555.Black, 7),
        };
        basis = selected.Value;
        independentMask = selected.Mask;
        return selected.Mask != 7;
    }
    /// <summary>Builds a neutral RGB5 highlight from its single intensity.</summary>
    /// <remarks>The repeated white at8D:D906 has equal red,green,blue.
    /// Expand one0..31 intensity into all three channels. Edited unequal
    /// channels remain independent overrides; no generated color is stored.</remarks>
    internal static Bgr555 Neutral(int intensity)
    {
        if ((uint)intensity > 31) throw new ArgumentOutOfRangeException(nameof(intensity));
        return new Bgr555(intensity, intensity, intensity);
    }
    /// <summary>Rotates the red endpoint into green by exchanging red and green channels.</summary>
    /// <remarks>Original projectile frame4 ink7 ($8D:D964) derives from
    /// frame0 ink3 ($D90C). Blue stays fixed; RGB5 channel exchange has no
    /// rounding,saturation or overflow. Differing asset values remain inputs.</remarks>
    internal static Bgr555 GreenFromRed(Bgr555 red) =>
        new(red.Green, red.Red, red.Blue);

    /// <summary>Selects middle projectile shades calculated from their adjacent inks.</summary>
    /// <remarks>In the original rows8D:D906+20*frame, red frame0 and magenta
    /// frame8 ink2 interpolate inks1/3; green frame4 and magenta frame8 ink5
    /// interpolate inks4/6. Each RGB5 channel rounds its midpoint upward.
    /// Other even-frame ink2 shades and frame6 ink5 also share matching
    /// midpoint channels; six differing components remain independent inputs
    /// covered by the class-level ink-color disposition. Mismatch alone is not its rationale.</remarks>
    internal static bool IsShadeMidpoint(int frame, int ink) =>
        ink == 2 && frame is 0 or 2 or 4 or 6 or 8 || ink == 5 && frame is 4 or 6 or 8;
    /// <summary>Blends red and white into the four red-frame highlight inks.</summary>
    /// <remarks>Original8D:D90E..D914 (frame0 inks4..7) are4/5,3/5,2/5,1/5
    /// white from ink0 toward red ink3. Round each RGB5 channel to nearest:
    /// (red*(5-weight)+white*weight+2)/5. There are no half ties; numerator
    /// is at most157,with no saturation or overflow. Independent source edits
    /// preserve supplied targets through input overrides,not a generated cache.</remarks>
    internal static Bgr555 RedHighlight(Bgr555 red, Bgr555 white, int ink)
    {
        if (ink is < 4 or > 7) throw new ArgumentOutOfRangeException(nameof(ink));
        int whiteWeight = 8 - ink, redWeight = 5 - whiteWeight;
        int r = ((red.Red) * redWeight + (white.Red) * whiteWeight + 2) / 5;
        int g = ((red.Green) * redWeight + (white.Green) * whiteWeight + 2) / 5;
        int b = ((red.Blue) * redWeight + (white.Blue) * whiteWeight + 2) / 5;
        return new Bgr555(r, g, b);
    }
    /// <summary>Asset filename for the Hyper Beam projectile palette colors, separate from Samus's full-body Hyper Beam cycle.</summary>
    public const string FileName = "hyper-beam-fx-colors.json";
    /// <summary>Supported revision of the ten-row RGB5 projectile-color JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Ten palette rows in the native <c>$8D:D900</c> cycle; stock even rows supply red, yellow, green, blue, and magenta endpoints, with intervening midpoint rows.</summary>
    public const int FrameCount = 10;
    /// <summary>Eight colors per native palette record, written to OBJ palette 6 colors 1..8; control words and frame duration are not included.</summary>
    public const int ColorsPerFrame = 8;
}
