using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Samus body colors during beam charge, pseudo-Screw, and Hyper-shot glow.</summary>
/// <remarks>Charge base rows9B9820/9920/9A20 exactly alias full-body bases
/// 9B9B20/9D20/9F20. After suit sharing their25 inputs are21 painted opaque
/// inks (Power1..15,Varia2/10/11,Gravity2/10/11),Gravity inks1/12 using the
/// already reviewed Power dim tint RG targets,and two unused slot-zero payloads.
/// Native91DD5B and the installed ApplyCharge copy these categorical sprite
/// inks to fixed OBJ slots. Ink index is not a lighting or brightness parameter;
/// a formula reciting these color choices would re-encode the painting. The
/// renderer skips index-zero pixels before RGB lookup,so the two transparent
/// payloads also have no rendering-derived numerical meaning. This is the
/// concrete1165 nonsense exception already established for the identical
/// full-body inputs,not a new exemption for all charge-related data. The fade
/// and repeated/suit views are calculated; separate assets keep independent
/// edits. Pseudo-Screw color generation and Hyper Beam remaining inputs have
/// their own dispositions and are not exempted here.</remarks>
public sealed class SamusChargeColorCatalog
{
    private readonly ChargeInputs chargedBeam;
    private readonly ChargeInputs pseudoScrew;
    private readonly SamusHyperBeamColorCatalog hyperShot;

    private SamusChargeColorCatalog(ChargeInputs chargedBeam,
        ChargeInputs pseudoScrew, SamusHyperBeamColorCatalog hyperShot)
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
        (pseudo ? pseudoScrew : chargedBeam).Resolve(suit, phase, color);

    public ushort ResolveHyper(int frame, int color)
    {
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return hyperShot.Resolve(SamusChargeColorFormat.HyperFrameCount - 1 - frame, color);
    }

    public void ApplyCharge(SnesCgram cgram, bool pseudo, int suit, int phase) =>
        Apply(cgram, pseudo ? pseudoScrew : chargedBeam, suit, phase);

    public void ApplyHyper(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + color, ResolveHyper(frame, color));
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
        return new(CompileCharge(document.ChargedBeam, "charged beam", false),
            CompileCharge(document.PseudoScrew, "pseudo-Screw", true),
            CompileHyper(document.HyperShot));
    }

    public static byte[] Write(SamusChargeColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Shares native repeated phases without caching calculated palette rows.</summary>
    /// <remarks>Bank91 lists D7DB/D7E7/D7F3 select charge shades0,1,2,3,2,1;
    /// D805/D811/D81D select the same pseudo-Screw row for phases0..2 and
    /// the normal row for3..5. Each differing supplied color remains independent.
    /// Charge shades1..3 use the shared eighth-step whitening calculation; differing
    /// channels remain editable inputs. Common base/pseudo-Screw inks share
    /// the corresponding Power Suit input; each suit-specific or edited value
    /// stays independent. Original charge bases9820/9920/9A20 differ only at
    /// Varia inks2/10/11 and Gravity0/1/2/10/11/12. Pseudo-Screw holds select
    /// bright and normal rows through91D805/D811/D81D. Their distinct color
    /// generation remains under review.</remarks>
    private sealed class ChargeInputs
    {
        private readonly bool pseudo;
        private readonly Dictionary<int, ushort> colors = new();
        private readonly Dictionary<int, LoadingPaletteInputView.Channels> fadeInputs = new();

        internal ChargeInputs(ushort[][][] source, bool pseudo)
        {
            this.pseudo = pseudo;
            for (int suit = 0; suit < source.Length; suit++)
            for (int phase = 0; phase < source[suit].Length; phase++)
            for (int color = 0; color < source[suit][phase].Length; color++)
            {
                int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
                ushort value = source[suit][phase][color];
                if (phase != canonical && value == source[suit][canonical][color]) continue;
                if (suit != 0 && phase == canonical && (pseudo || phase == 0) && value == source[0][phase][color]) continue;
                int key = (suit * 6 + phase) * 16 + color;
                if (!pseudo && phase == canonical && phase != 0)
                {
                    ushort expected = SamusPaletteFade.EighthTowardWhite(source[suit][0][color], phase);
                    if (value != expected) fadeInputs.Add(key, new(value, expected));
                }
                else colors.Add(key, value);
            }
        }

        internal ushort Resolve(int suit, int phase, int color)
        {
            if ((uint)suit >= SamusChargeColorFormat.SuitCount) throw new ArgumentOutOfRangeException(nameof(suit));
            int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
            if ((uint)color >= SamusChargeColorFormat.ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
            int key = (suit * 6 + phase) * 16 + color;
            if (colors.TryGetValue(key, out ushort value)) return value;
            if (phase != canonical) return Resolve(suit, canonical, color);
            if (suit != 0 && (pseudo || phase == 0)) return Resolve(0, phase, color);
            ushort expected = SamusPaletteFade.EighthTowardWhite(Resolve(suit, 0, color), phase);
            return fadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
        }
    }

    private static void Apply(SnesCgram cgram, ChargeInputs source, int suit, int phase)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + color,
                source.Resolve(suit, phase, color));
    }
    private static ChargeInputs CompileCharge(PaletteRgb5[][][]? source, string name, bool pseudo)
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
        return new(result, pseudo);
    }

    /// <summary>Uses the same palette algorithms for the reverse Hyper-shot playback view.</summary>
    /// <remarks>Original91D82B..D83D and91D99E..D9B0 contain the same
    /// ten pointers. Shot playback decrements its table offset from20 to2,
    /// reversing the full-body cycle. Inputs remain asset-local: edits to the
    /// shot asset do not alter the separately supplied full-body cycle.</remarks>
    private static SamusHyperBeamColorCatalog CompileHyper(PaletteRgb5[][]? source)
    {
        if (source is null || source.Length != SamusChargeColorFormat.HyperFrameCount)
            throw new InvalidDataException("Hyper shot requires ten frames.");
        var cycleOrder = new PaletteRgb5[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
            cycleOrder[source.Length - 1 - frame] = source[frame];
        return SamusHyperBeamColorCatalog.FromFrames(cycleOrder);
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
    /// <summary>Returns the first phase using a native charge palette.</summary>
    /// <remarks>Phase0..5: charge mirrors around3; pseudo-Screw holds each
    /// input for three phases. Original pointers91D7DB..D827 establish exact
    /// row identity for all three suits. No extrapolation or invalid masking.</remarks>
    internal static int CanonicalPhase(bool pseudo, int phase)
    {
        if ((uint)phase >= PhasesPerSuit) throw new ArgumentOutOfRangeException(nameof(phase));
        return pseudo ? phase / 3 * 3 : Math.Min(phase, 6 - phase);
    }
    public const string FileName = "samus-charge-colors.json";
    public const int Version = 1;
    public const int SuitCount = 3;
    public const int PhasesPerSuit = 6;
    public const int HyperFrameCount = 10;
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
