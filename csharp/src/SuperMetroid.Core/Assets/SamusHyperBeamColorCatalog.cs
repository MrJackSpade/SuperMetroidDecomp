using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable ten-frame full-body Hyper Beam RGB5 palette cycle.</summary>
public sealed class SamusHyperBeamColorCatalog
{
    private readonly Dictionary<int, ushort> colors = new();

    private SamusHyperBeamColorCatalog(ushort[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        {
            int index = frame * 16 + color, source = SamusHyperBeamColorFormat.CanonicalColorIndex(frame, color);
            if (source != index && frames[frame][color] == frames[source / 16][source % 16]) continue;
            if (source == index && frame == 7 && color != 0 &&
                frames[frame][color] == SamusHyperBeamColorFormat.YellowFromGreen(frames[5][color])) continue;
            if (source == index && frame == 6 && color != 0 && color is not (1 or 8 or 11) &&
                frames[frame][color] == SamusHyperBeamColorFormat.GreenYellowMidpoint(frames[5][color])) continue;
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
        if (document.Version != SamusHyperBeamColorFormat.Version ||
            document.Frames is null || document.Frames.Length != SamusHyperBeamColorFormat.FrameCount)
            throw new InvalidDataException("Samus Hyper Beam colors require the supported version and ten frames.");
        var compiled = new ushort[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
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
        return frame == 6 ? SamusHyperBeamColorFormat.GreenYellowMidpoint(Resolve(5, color)) :
            SamusHyperBeamColorFormat.YellowFromGreen(Resolve(5, color));
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

    /// <summary>Interpolates red halfway from green-frame red to its green value, rounding upward.</summary>
    /// <remarks>Original frame6 ($9B:A2A0) preserves frame5 green/blue;
    /// nine canonical opaque inks also have red=ceil((red5+green5)/2).
    /// Slots1/8/11 differ in red and remain outside this whole-word conversion
    /// pending component review. The numerator is at most63; no saturation or
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
