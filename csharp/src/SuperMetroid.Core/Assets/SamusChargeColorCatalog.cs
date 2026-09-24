using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Samus body colors during beam charge, pseudo-Screw, and Hyper-shot glow.</summary>
public sealed class SamusChargeColorCatalog
{
    private readonly ushort[][][] chargedBeam;
    private readonly ushort[][][] pseudoScrew;
    private readonly ushort[][] hyperShot;

    private SamusChargeColorCatalog(ushort[][][] chargedBeam,
        ushort[][][] pseudoScrew, ushort[][] hyperShot)
    {
        this.chargedBeam = chargedBeam;
        this.pseudoScrew = pseudoScrew;
        this.hyperShot = hyperShot;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveCharge(bool pseudo, int suit, int phase, int color) =>
        Resolve(pseudo ? pseudoScrew : chargedBeam, suit, phase, color);

    public ushort ResolveHyper(int frame, int color)
    {
        if ((uint)frame >= hyperShot.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= hyperShot[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
        return hyperShot[frame][color];
    }

    public void ApplyCharge(SnesCgram cgram, bool pseudo, int suit, int phase) =>
        Apply(cgram, pseudo ? pseudoScrew : chargedBeam, suit, phase);

    public void ApplyHyper(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= hyperShot.Length) throw new ArgumentOutOfRangeException(nameof(frame));
        Apply(cgram, hyperShot[frame]);
    }

    public static SamusChargeColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusChargeColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusChargeColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus charge color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus charge color JSON.", error);
        }
        if (document.Version != SamusChargeColorFormat.Version)
            throw new InvalidDataException("Samus charge colors require the supported version.");
        return new(CompileCharge(document.ChargedBeam, "charged beam"),
            CompileCharge(document.PseudoScrew, "pseudo-Screw"),
            CompileHyper(document.HyperShot));
    }

    public static byte[] Write(SamusChargeColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Resolve(ushort[][][] source, int suit, int phase, int color)
    {
        if ((uint)suit >= source.Length) throw new ArgumentOutOfRangeException(nameof(suit));
        if ((uint)phase >= source[suit].Length) throw new ArgumentOutOfRangeException(nameof(phase));
        if ((uint)color >= source[suit][phase].Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return source[suit][phase][color];
    }

    private static void Apply(SnesCgram cgram, ushort[][][] source, int suit, int phase)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)suit >= source.Length) throw new ArgumentOutOfRangeException(nameof(suit));
        if ((uint)phase >= source[suit].Length) throw new ArgumentOutOfRangeException(nameof(phase));
        Apply(cgram, source[suit][phase]);
    }

    private static void Apply(SnesCgram cgram, ushort[] colors)
    {
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index,
                colors[index]);
    }

    private static ushort[][][] CompileCharge(PaletteRgb5[][][]? source, string name)
    {
        if (source is null || source.Length != SamusChargeColorFormat.SuitCount)
            throw new InvalidDataException($"{name} requires three suits.");
        var result = new ushort[source.Length][][];
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? phases = source[suit];
            if (phases is null || phases.Length != SamusChargeColorFormat.PhasesPerSuit)
                throw new InvalidDataException($"{name} suit {suit} requires six phases.");
            result[suit] = new ushort[phases.Length][];
            for (int phase = 0; phase < phases.Length; phase++)
                result[suit][phase] = CompileColors(phases[phase],
                    $"{name} suit {suit}, phase {phase}");
        }
        return result;
    }

    private static ushort[][] CompileHyper(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != SamusChargeColorFormat.HyperFrameCount)
            throw new InvalidDataException("Hyper shot requires ten frames.");
        var result = new ushort[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
            result[frame] = CompileColors(source[frame], $"Hyper shot frame {frame}");
        return result;
    }

    private static ushort[] CompileColors(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusChargeColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"{name} requires sixteen colors.");
        var result = new ushort[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException($"{name} color {index} requires RGB5 channels 0..31.");
            result[index] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Samus charge color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record SamusChargeColorDocument
{
    public required int Version { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] ChargedBeam { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] PseudoScrew { get; init; }
    /// <summary>Ten Hyper-shot glow frames in playback order.</summary>
    public required PaletteRgb5[][] HyperShot { get; init; }
}

public static class SamusChargeColorFormat
{
    public const string FileName = "samus-charge-colors.json";
    public const int Version = 1;
    public const int SuitCount = 3;
    public const int PhasesPerSuit = 6;
    public const int HyperFrameCount = 10;
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
