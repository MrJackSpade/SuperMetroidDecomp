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
/// edits. Pseudo-Screw normal rows are exactly9400/9520/9800: the24 inputs
/// are the normal catalog's22 painted inks and two unused transparent payloads.
/// Its calculated bright phase retains only Power/Varia ink2 blue,identical
/// to the already reviewed full-body bright/middle ink2 targets at9C84/9C64
/// (Varia9E84 equals9C64). Those chosen tint components have the same concrete
/// painting rationale,not an unimplemented numerical curve. This resolves
/// charge/pseudo-Screw color payloads only; Hyper Beam remaining inputs and
/// other selectors/timing require their own dispositions.</remarks>
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

    /// <summary>Gets packed RGB5 ink 0–15 from Hyper-shot playback frame 0–9 in the native $91:D82B reverse-cycle pointer order; this asset's edits are independent of the full-body Hyper cycle.</summary>
    public ushort ResolveHyper(int frame, int color)
    {
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return hyperShot.Resolve(SamusChargeColorFormat.HyperFrameCount - 1 - frame, color);
    }

    /// <summary>Installs all sixteen selected charge or pseudo-Screw inks at CGRAM 192–207, including the retained transparent-slot word; charge admission and phase advancement remain Samus behavior.</summary>
    /// <param name="cgram">Current Samus OBJ palette destination.</param>
    /// <param name="pseudo">True for pseudo-Screw's bright/normal holds; false for the charged-beam whitening pulse.</param>
    /// <param name="suit">Suit index: Power 0, Varia 1, Gravity 2.</param>
    /// <param name="phase">Playback phase 0–5; stock charge shades follow 0/1/2/3/2/1, while pseudo-Screw holds bright for 0–2 and normal for 3–5.</param>
    public void ApplyCharge(SnesCgram cgram, bool pseudo, int suit, int phase) =>
        Apply(cgram, pseudo ? pseudoScrew : chargedBeam, suit, phase);

    /// <summary>Installs all sixteen Hyper-shot inks at CGRAM 192–207 for playback frame 0–9; shot-glow timing and the native decrementing palette cursor remain engine-owned.</summary>
    public void ApplyHyper(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + color, ResolveHyper(frame, color));
    }

    /// <summary>Loads version-1 <c>samus-charge-colors.json</c>, validating three suits with six phases each, ten Hyper-shot frames, sixteen colors per row, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled selected colors, sharing stock repeated phases and calculated fades only when supplied values agree.</returns>
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

    /// <summary>Serializes the charge/pseudo-Screw/Hyper-shot colors as indented camel-case UTF-8 JSON, validating schema, array dimensions, and RGB5 channel bounds before returning the bytes.</summary>
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
    /// bright and normal rows through91D805/D811/D81D. Bright colors reuse
    /// the active-shinespark operations; Power/Varia ink2 retain distinct blue
    /// inputs. All bright transparent words share Power normal color0.</remarks>
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
                if (pseudo && phase == 0)
                {
                    ushort expected = color == 0 ? source[0][3][0] :
                        SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, source[suit][3][color]);
                    if (value != expected) fadeInputs.Add((suit * 6 + phase) * 16 + color, new(value, expected));
                    continue;
                }
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
            if (pseudo && phase == 0)
            {
                ushort bright = color == 0 ? Resolve(0, 3, 0) :
                    SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, Resolve(suit, 3, color));
                return fadeInputs.TryGetValue(key, out var brightInput) ? brightInput.Apply(bright) : bright;
            }
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus charge color property {name}."));
}

/// <summary>Editable RGB5 body-palette rows for charge, pseudo-Screw, and Hyper-shot playback; each row contains sixteen native OBJ slots, including the nonvisible transparent slot.</summary>
public sealed record SamusChargeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] ChargedBeam { get; init; }
    /// <summary>Power/Varia/Gravity suits, each with six playback phases.</summary>
    public required PaletteRgb5[][][] PseudoScrew { get; init; }
    /// <summary>Ten Hyper-shot glow frames in playback order.</summary>
    public required PaletteRgb5[][] HyperShot { get; init; }
}

/// <summary>Installed filename and bounded suit, phase, frame, and palette geometry for selected Samus charge-related body colors.</summary>
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
    /// <summary>Calculates the bright pseudo-Screw shade from the corresponding normal ink.</summary>
    /// <remarks>Native91D805/D811/D81D select active-shinespark final rows
    /// 9B9C80/9E80/A080. Opaque gold inks reuse the owning full-body gold
    /// operation at shade3; other inks use the shared warm tint (RG+26,B+10),
    /// saturating each channel at31. These also cover common secondary-suit
    /// inks represented by Power aliases in the full-body view. Gravity's
    /// different base1/12 RG inputs both saturate here,so its normal-row RGB
    /// produces the identical final color. Power/Varia ink2 blue differs and
    /// remains independently supplied; all matching channels are calculated.</remarks>
    internal static ushort PseudoScrewBrightColor(int suit, int color, ushort basis)
    {
        if ((uint)suit >= SuitCount) throw new ArgumentOutOfRangeException(nameof(suit));
        if (color is < 1 or >= ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
        if (basis > 0x7fff) throw new ArgumentOutOfRangeException(nameof(basis));
        return SamusFullBodyCycleColorFormat.TryActiveGoldRamp(suit * 16 + 11, color, basis, out ushort gold)
            ? gold : SamusFullBodyCycleColorFormat.ActiveShineTint(basis, 3);
    }
    /// <summary>Installed editable JSON filename for charged-beam, pseudo-Screw, and Hyper-shot palette rows.</summary>
    public const string FileName = "samus-charge-colors.json";
    /// <summary>Supported color schema revision, requiring the complete suit/phase and Hyper-shot row sets.</summary>
    public const int Version = 1;
    /// <summary>Three ordered suit variants: Power, Varia, and Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Six playback phases per suit, matching native charge lists $91:D7DB/$D7E7/$D7F3 and pseudo-Screw lists $D805/$D811/$D81D.</summary>
    public const int PhasesPerSuit = 6;
    /// <summary>Ten Hyper-shot playback frames in the native $91:D82B–$D83D reverse palette sequence.</summary>
    public const int HyperFrameCount = 10;
    /// <summary>Sixteen RGB5 words per Samus OBJ palette row, including the retained transparent-slot word and fifteen visible inks.</summary>
    public const int ColorsPerPalette = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
