using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Hyper Beam projectile hues with shared white and calculated intermediate frames.</summary>
public sealed class HyperBeamFxColorCatalog
{
    private readonly Dictionary<int, ushort> colors = new();
    private readonly Dictionary<int, PairedChannels> pairedInputs = new();

    private HyperBeamFxColorCatalog(ushort[][] frames)
    {
        for (int frame = 0; frame < HyperBeamFxColorFormat.FrameCount; frame++)
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
        {
            ushort value = frames[frame][color];
            if (color == 0 && frame != 0 && value == frames[0][0]) continue;
            if (color != 0 && (frame & 1) != 0 && value == SamusHyperBeamColorFormat.HueMidpoint(
                frames[frame - 1][color], frames[(frame + 1) % HyperBeamFxColorFormat.FrameCount][color])) continue;
            if (frame == 2 && color >= 4 && value == SamusHyperBeamColorFormat.YellowFromGreen(frames[4][color])) continue;
            if (frame == 0 && color >= 4 && value == HyperBeamFxColorFormat.RedHighlight(frames[0][3], frames[0][0], color)) continue;
            if (HyperBeamFxColorFormat.IsShadeMidpoint(frame, color) && value == SamusHyperBeamColorFormat.HueMidpoint(
                frames[frame][color - 1], frames[frame][color + 1])) continue;
            if (HasPairedChannels(frame, color))
            {
                pairedInputs.Add(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, new(value, frame == 0));
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
    /// body cycle. Other even-frame endpoint relationships remain under review.</remarks>
    private ushort Resolve(int frame, int color)
    {
        if (colors.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out ushort value)) return value;
        if (pairedInputs.TryGetValue(frame * HyperBeamFxColorFormat.ColorsPerFrame + color, out var paired)) return paired.Resolve(frame == 0);
        if (color == 0) return colors[0];
        if (HyperBeamFxColorFormat.IsShadeMidpoint(frame, color))
            return SamusHyperBeamColorFormat.HueMidpoint(Resolve(frame, color - 1), Resolve(frame, color + 1));
        if (frame == 0) return HyperBeamFxColorFormat.RedHighlight(Resolve(0, 3), Resolve(0, 0), color);
        if (frame == 2) return SamusHyperBeamColorFormat.YellowFromGreen(Resolve(4, color));
        return SamusHyperBeamColorFormat.HueMidpoint(
            Resolve(frame - 1, color), Resolve((frame + 1) % HyperBeamFxColorFormat.FrameCount, color));
    }

    /// <summary>Selects the red, green and magenta endpoint inks with two equal channels.</summary>
    /// <remarks>Original red frame0 inks3..7 have blue=green; green frame4
    /// inks4..7 and magenta frame8 inks1..7 have blue=red. Frame addresses
    /// start8D:D906 and advance20 bytes. These are hue-channel equalities;
    /// their remaining independent intensities still require review.</remarks>
    private static bool HasPairedChannels(int frame, int color) =>
        frame == 0 && color >= 3 || frame == 4 && color >= 4 || frame == 8 && color != 0;

    internal readonly struct PairedChannels
    {
        private readonly int red;
        private readonly int green;
        private readonly int? blue;

        internal PairedChannels(ushort supplied, bool redHue)
        {
            red = supplied & 31;
            green = supplied >> 5 & 31;
            int expectedBlue = redHue ? green : red;
            blue = (supplied >> 10 & 31) == expectedBlue ? null : supplied >> 10 & 31;
        }

        internal ushort Resolve(bool redHue) =>
            (ushort)(red | green << 5 | (blue ?? (redHue ? green : red)) << 10);
    }
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

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

        var compiled = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (colors is null || colors.Length != HyperBeamFxColorFormat.ColorsPerFrame)
                throw new InvalidDataException($"Hyper Beam FX frame {frame} requires eight colors.");
            compiled[frame] = new ushort[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Hyper Beam FX frame {frame}, color {color} requires RGB5 components.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new(compiled);
    }

    public void Apply(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= HyperBeamFxColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < HyperBeamFxColorFormat.ColorsPerFrame; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    public static byte[] Write(HyperBeamFxColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicateProperties(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException("Duplicate Hyper Beam FX color property.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicateProperties(child);
    }
}

public sealed record HyperBeamFxColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Presentation geometry of the color payloads at $8D:D906..D9CA.</summary>
public static class HyperBeamFxColorFormat
{
    /// <summary>Selects middle projectile shades calculated from their adjacent inks.</summary>
    /// <remarks>In the original rows8D:D906+20*frame, red frame0 and magenta
    /// frame8 ink2 interpolate inks1/3; green frame4 and magenta frame8 ink5
    /// interpolate inks4/6. Each RGB5 channel rounds its midpoint upward.
    /// Other shade relationships remain under independent review.</remarks>
    internal static bool IsShadeMidpoint(int frame, int ink) =>
        ink == 2 && frame is 0 or 8 || ink == 5 && frame is 4 or 8;
    /// <summary>Blends red and white into the four red-frame highlight inks.</summary>
    /// <remarks>Original8D:D90E..D914 (frame0 inks4..7) are4/5,3/5,2/5,1/5
    /// white from ink0 toward red ink3. Round each RGB5 channel to nearest:
    /// (red*(5-weight)+white*weight+2)/5. There are no half ties; numerator
    /// is at most157,with no saturation or overflow. Independent source edits
    /// preserve supplied targets through input overrides,not a generated cache.</remarks>
    internal static ushort RedHighlight(ushort red, ushort white, int ink)
    {
        if (ink is < 4 or > 7) throw new ArgumentOutOfRangeException(nameof(ink));
        int whiteWeight = 8 - ink, redWeight = 5 - whiteWeight;
        int r = ((red & 31) * redWeight + (white & 31) * whiteWeight + 2) / 5;
        int g = ((red >> 5 & 31) * redWeight + (white >> 5 & 31) * whiteWeight + 2) / 5;
        int b = ((red >> 10 & 31) * redWeight + (white >> 10 & 31) * whiteWeight + 2) / 5;
        return (ushort)(r | g << 5 | b << 10);
    }
    public const string FileName = "hyper-beam-fx-colors.json";
    public const int Version = 1;
    public const int FrameCount = 10;
    public const int ColorsPerFrame = 8;
}
