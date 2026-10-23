using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

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
    /// <summary>Charge pulse rows share suit bases and calculated whitening unless the asset supplies independent edits.</summary>
    internal readonly ChargeInputs chargedBeam;
    /// <summary>Pseudo-Screw rows share their repeated holds and selected bright tint with normal suit inks.</summary>
    internal readonly ChargeInputs pseudoScrew;
    /// <summary>Shot-glow playback uses its own Hyper color inputs, independent of the full-body cycle asset.</summary>
    private readonly SamusHyperBeamColorCatalog hyperShot;

    /// <summary>Combines the two six-phase suit views with the separately editable reverse Hyper-shot view.</summary>
    private SamusChargeColorCatalog(ChargeInputs chargedBeam,
        ChargeInputs pseudoScrew, SamusHyperBeamColorCatalog hyperShot)
    {
        this.chargedBeam = chargedBeam;
        this.pseudoScrew = pseudoScrew;
        this.hyperShot = hyperShot;
    }

    /// <summary>Gets packed RGB5 ink 0–15 from Hyper-shot playback frame 0–9 in the native $91:D82B reverse-cycle pointer order; this asset's edits are independent of the full-body Hyper cycle.</summary>
    public Bgr555 ResolveHyper(int frame, int color)
    {
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return hyperShot.Resolve(SamusChargeColorFormat.HyperFrameCount - 1 - frame, color);
    }

    /// <summary>Installs all sixteen selected charge or pseudo-Screw inks at CGRAM 192–207, including the retained transparent-slot word; charge admission and phase advancement remain Samus behavior.</summary>
    /// <param name="colours">Current Samus OBJ palette destination.</param>
    /// <param name="pseudo">True for pseudo-Screw's bright/normal holds; false for the charged-beam whitening pulse.</param>
    /// <param name="suit">Suit index: Power 0, Varia 1, Gravity 2.</param>
    /// <param name="phase">Playback phase 0–5; stock charge shades follow 0/1/2/3/2/1, while pseudo-Screw holds bright for 0–2 and normal for 3–5.</param>
    public void ApplyCharge(SamusColours colours, bool pseudo, int suit, int phase) =>
        Apply(colours, pseudo ? pseudoScrew : chargedBeam, suit, phase);

    /// <summary>Installs all sixteen Hyper-shot inks at CGRAM 192–207 for playback frame 0–9; shot-glow timing and the native decrementing palette cursor remain engine-owned.</summary>
    public void ApplyHyper(SamusColours colours, int frame)
    {
        Ensure.NotNull(colours);
        if ((uint)frame >= SamusChargeColorFormat.HyperFrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            colours.Suit.Write(color, ResolveHyper(frame, color));
    }

    /// <summary>Loads version-1 <c>samus-charge-colors.json</c>, validating three suits with six phases each, ten Hyper-shot frames, sixteen colors per row, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled selected colors, sharing stock repeated phases and calculated fades only when supplied values agree.</returns>
    public static SamusChargeColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        SamusChargeColorDocument document;
        document = JsonAssetDocument.Read<SamusChargeColorDocument>(json, ArtworkJson.Options, "Samus charge color");
        if (document.Version != SamusChargeColorFormat.Version)
            throw new InvalidDataException("Samus charge colors require the supported version.");
        return new(CompileCharge(document.ChargedBeam, "charged beam", SamusChargeKind.ChargedBeam),
            CompileCharge(document.PseudoScrew, "pseudo-Screw", SamusChargeKind.PseudoScrew),
            CompileHyper(document.HyperShot));
    }

    /// <summary>Serializes the charge/pseudo-Screw/Hyper-shot colors as indented camel-case UTF-8 JSON, validating schema, array dimensions, and RGB5 channel bounds before returning the bytes.</summary>
    public static byte[] Write(SamusChargeColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

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
    internal sealed class ChargeInputs
    {
        /// <summary>Selects three-phase pseudo-Screw holds instead of the mirrored charge pulse.</summary>
        private readonly bool pseudo;
        /// <summary>Palette cells whose supplied inks cannot be shared with a canonical suit or phase.</summary>
        internal readonly Dictionary<ColourCell, Bgr555> colors = new();
        /// <summary>Per-channel deviations from the calculated whitening or pseudo-Screw bright tint.</summary>
        internal readonly Dictionary<int, LoadingPaletteInputView.Channels> fadeInputs = new();

        /// <summary>Reduces each supplied row to its independent colors and channel differences while preserving divergent repeated phases.</summary>
        internal ChargeInputs(Bgr555[][][] source, bool pseudo)
        {
            this.pseudo = pseudo;
            for (int suit = 0; suit < source.Length; suit++)
            for (int phase = 0; phase < source[suit].Length; phase++)
            for (int color = 0; color < source[suit][phase].Length; color++)
            {
                int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
                Bgr555 value = source[suit][phase][color];
                if (phase != canonical && value == source[suit][canonical][color]) continue;
                if (pseudo && phase == 0)
                {
                    Bgr555 expected = color == 0 ? source[0][3][0] :
                        SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, source[suit][3][color]);
                    if (value != expected) fadeInputs.Add((suit * 6 + phase) * 16 + color, new(value, expected));
                    continue;
                }
                if (suit != 0 && phase == canonical && (pseudo || phase == 0) && value == source[0][phase][color]) continue;
                int key = (suit * 6 + phase) * 16 + color;
                var keyCell = new ColourCell((suit * 6 + phase), color);
                if (!pseudo && phase == canonical && phase != 0)
                {
                    Bgr555 expected = SamusPaletteFade.EighthTowardWhite(source[suit][0][color], phase);
                    if (value != expected) fadeInputs.Add(key, new(value, expected));
                }
                else colors.Add(keyCell, value);
            }
        }

        /// <summary>Restores one edited or shared ink through canonical phase, suit alias, and calculated tint relationships.</summary>
        internal Bgr555 Resolve(int suit, int phase, int color)
        {
            if ((uint)suit >= SamusChargeColorFormat.SuitCount) throw new ArgumentOutOfRangeException(nameof(suit));
            int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
            if ((uint)color >= SamusChargeColorFormat.ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
            int key = (suit * 6 + phase) * 16 + color;
            var keyCell = new ColourCell((suit * 6 + phase), color);
            if (colors.TryGetValue(keyCell, out Bgr555 value)) return value;
            if (phase != canonical) return Resolve(suit, canonical, color);
            if (pseudo && phase == 0)
            {
                Bgr555 bright = color == 0 ? Resolve(0, 3, 0) :
                    SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, Resolve(suit, 3, color));
                return fadeInputs.TryGetValue(key, out var brightInput) ? brightInput.Apply(bright) : bright;
            }
            if (suit != 0 && (pseudo || phase == 0)) return Resolve(0, phase, color);
            Bgr555 expected = SamusPaletteFade.EighthTowardWhite(Resolve(suit, 0, color), phase);
            return fadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
        }
    }

    /// <summary>Copies the selected sixteen-ink row into Samus's OBJ palette without altering phase timing.</summary>
    private static void Apply(SamusColours colours, ChargeInputs source, int suit, int phase)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            colours.Suit.Write(color,
                source.Resolve(suit, phase, color));
    }
    /// <summary>Requires three suits and six complete RGB5 rows per suit before building the charge or pseudo-Screw view.</summary>
    private static ChargeInputs CompileCharge(PaletteRgb5[][][]? source, string name, SamusChargeKind kind)
    {
        if (source is null || source.Length != SamusChargeColorFormat.SuitCount)
            throw new InvalidDataException($"{name} requires three suits.");
        var result = new Bgr555[source.Length][][];
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? phases = source[suit];
            if (phases is null || phases.Length != SamusChargeColorFormat.PhasesPerSuit)
                throw new InvalidDataException($"{name} suit {suit} requires six phases.");
            result[suit] = new Bgr555[phases.Length][];
            for (int phase = 0; phase < phases.Length; phase++)
                result[suit][phase] = CompileColors(phases[phase],
                    $"{name} suit {suit}, phase {phase}");
        }
        return new(result, kind == SamusChargeKind.PseudoScrew);
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
    /// <summary>Requires all sixteen palette slots and five-bit RGB channels before packing a row as BGR555.</summary>
    private static Bgr555[] CompileColors(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusChargeColorFormat.ColorsPerPalette)
            throw new InvalidDataException($"{name} requires sixteen colors.");
        var result = new Bgr555[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? rgb = source[index];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException($"{name} color {index} requires RGB5 channels 0..31.");
            result[index] = rgb.ToBgr555();
        }
        return result;
    }

}
