using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

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
    /// <summary>Exact supplied colors that differ from every derived hue and shade relationship.</summary>
    internal readonly Dictionary<int, Bgr555> colors = new();
    /// <summary>Base RGB5 intensity shared by the stock neutral highlight channels.</summary>
    internal readonly int neutralIntensity;
    /// <summary>Independent edits to the neutral highlight's otherwise equal channels.</summary>
    internal readonly LoadingPaletteInputView.Channels neutralOverrides;
    /// <summary>Red, green, and magenta endpoint inks with paired hue channels.</summary>
    internal readonly Dictionary<int, PairedChannels> pairedInputs = new();
    /// <summary>Edited independent channels of endpoint inks sharing hue extrema.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> endpointInputs = new();
    /// <summary>Edited midpoint-shade channels preserved beyond calculated neighbors.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> shadeInputs = new();

    /// <summary>Separates supplied palette rows into canonical relationships and exact independent edits.</summary>
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
    private static HyperBeamEndpointChannels SharedEndpointChannels(int frame, int color, Bgr555 white, Bgr555 red) =>
        frame == 0 ? new HyperBeamEndpointChannels(white.Red, null) :
        frame == 4 ? new HyperBeamEndpointChannels(null, red.Red) :
        color is 3 or 7 ? new HyperBeamEndpointChannels(null, red.Green) : new HyperBeamEndpointChannels(red.Red, null);
    /// <summary>Stores independent endpoint channels while deriving the paired blue hue channel.</summary>
    internal readonly struct PairedChannels
    {
        /// <summary>Edited red channel, or null when it shares the selected source.</summary>
        private readonly int? red;
        /// <summary>Edited green channel, or null when it shares the selected source.</summary>
        private readonly int? green;
        /// <summary>Edited blue channel, or null when it matches the paired red or green channel.</summary>
        internal readonly int? blue;

        /// <summary>Extracts only channels that differ from the shared red or green hue basis.</summary>
        internal PairedChannels(Bgr555 supplied, bool redHue, int? sharedRed = null, int? sharedGreen = null)
        {
            int suppliedRed = supplied.Red, suppliedGreen = supplied.Green;
            red = suppliedRed == sharedRed ? null : suppliedRed;
            green = suppliedGreen == sharedGreen ? null : suppliedGreen;
            int expectedBlue = redHue ? suppliedGreen : suppliedRed;
            blue = (supplied.Blue) == expectedBlue ? null : supplied.Blue;
        }

        /// <summary>Recombines edited channels with shared hue extrema and the paired blue channel.</summary>
        internal Bgr555 Resolve(bool redHue, int sharedRed = 0, int sharedGreen = 0)
        {
            int resolvedRed = red ?? sharedRed, resolvedGreen = green ?? sharedGreen;
            return new(resolvedRed, resolvedGreen, blue ?? (redHue ? resolvedGreen : resolvedRed));
        }
    }
    /// <summary>Strict camel-case JSON settings for the editable Hyper Beam FX palette.</summary>
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
            !HyperBeamFxColorFormat.Shape.HasRows(document.Frames))
            throw new InvalidDataException("Hyper Beam FX colors require ten frames at the supported version.");

        var compiled = new Bgr555[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (!HyperBeamFxColorFormat.Shape.HasColumns(colors))
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
    /// <param name="destination">The colours the cycle shows, eight of them.</param>
    /// <param name="frame">Zero-based palette row, from 0 through 9.</param>
    public void Apply(ColourRun destination, int frame)
    {
        if ((uint)frame >= HyperBeamFxColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        Ensure.LengthEqual(destination.Colors, HyperBeamFxColorFormat.ColorsPerFrame);
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
            destination.Write(color, Resolve(frame, color));
    }

    /// <summary>Serializes the projectile-color document as indented camel-case UTF-8 JSON, then validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all ten eight-color RGB5 rows.</param>
    /// <returns>Validated JSON bytes suitable for the projectile-color asset file.</returns>
    public static byte[] Write(HyperBeamFxColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Load(json));

    /// <summary>Rejects duplicate palette properties before document deserialization.</summary>
    private static void RejectDuplicateProperties(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Hyper Beam FX color property."));
}
