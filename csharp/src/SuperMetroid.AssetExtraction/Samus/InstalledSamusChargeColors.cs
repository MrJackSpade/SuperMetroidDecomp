using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus;

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
public sealed class InstalledSamusChargeColors
{
    /// <summary>Charge pulse rows share suit bases and calculated whitening unless the asset supplies independent edits.</summary>
    internal readonly ChargeInputs chargedBeam;
    /// <summary>Pseudo-Screw rows share their repeated holds and selected bright tint with normal suit inks.</summary>
    internal readonly ChargeInputs pseudoScrew;
    /// <summary>Shot-glow playback uses its own Hyper color inputs, independent of the full-body cycle asset.</summary>
    private readonly InstalledSamusHyperBeamColors hyperShot;

    /// <summary>Combines the two six-phase suit views with the separately editable reverse Hyper-shot view.</summary>
    /// <param name="chargedBeam">Three suits of six charged-beam phase rows.</param>
    /// <param name="pseudoScrew">Three suits of six pseudo-Screw phase rows.</param>
    /// <param name="hyperShot">The Hyper-shot rows in full-body cycle order.</param>
    internal InstalledSamusChargeColors(RgbColor[][][] chargedBeam,
        RgbColor[][][] pseudoScrew, InstalledSamusHyperBeamColors hyperShot)
    {
        this.chargedBeam = new(chargedBeam, SamusChargeKind.ChargedBeam);
        this.pseudoScrew = new(pseudoScrew, SamusChargeKind.PseudoScrew);
        this.hyperShot = hyperShot;
    }

    /// <summary>
    /// The decoded colours keyed by what they show (#1464): every charge palette's suit colours, by sequence, suit and
    /// step, and the Hyper-shot cycle. The document's suit-then-step row order ends here.
    /// </summary>
    public SamusChargeColours ToColours()
    {
        var palettes = new Dictionary<SamusChargePalette, ImmutableArray<RgbColor>>();
        foreach (SamusSuit suit in SamusSuitDefinitions.ListOrder)
        for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
        {
            palettes.Add(new(SamusChargeKind.ChargedBeam, suit, phase), Row(chargedBeam, suit, phase));
            palettes.Add(new(SamusChargeKind.PseudoScrew, suit, phase), Row(pseudoScrew, suit, phase));
        }
        return new(palettes, hyperShot.ToColours());
    }

    /// <summary>The suit colours <paramref name="source"/> resolves for <paramref name="suit"/> at <paramref name="phase"/>.</summary>
    private static ImmutableArray<RgbColor> Row(ChargeInputs source, SamusSuit suit, int phase) =>
        [.. Enumerable.Range(0, SamusChargeColorFormat.ColorsPerPalette)
            .Select(color => source.Resolve(suit.ListPlace(), phase, color))];

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
        internal readonly Dictionary<ColourCell, RgbColor> colors = new();
        /// <summary>Per-channel deviations from the calculated whitening or pseudo-Screw bright tint.</summary>
        internal readonly Dictionary<int, LoadingPaletteInputView.Channels> fadeInputs = new();

        /// <summary>Reduces each supplied row to its independent colors and channel differences while preserving divergent repeated phases.</summary>
        internal ChargeInputs(RgbColor[][][] source, SamusChargeKind kind)
        {
            pseudo = kind == SamusChargeKind.PseudoScrew;
            for (int suit = 0; suit < source.Length; suit++)
            for (int phase = 0; phase < source[suit].Length; phase++)
            for (int color = 0; color < source[suit][phase].Length; color++)
            {
                int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
                RgbColor value = source[suit][phase][color];
                if (phase != canonical && value == source[suit][canonical][color]) continue;
                if (pseudo && phase == 0)
                {
                    RgbColor expected = color == 0 ? source[0][3][0] :
                        SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, source[suit][3][color]);
                    if (value != expected) fadeInputs.Add((suit * 6 + phase) * 16 + color, new(value, expected));
                    continue;
                }
                if (suit != 0 && phase == canonical && (pseudo || phase == 0) && value == source[0][phase][color]) continue;
                int key = (suit * 6 + phase) * 16 + color;
                var keyCell = new ColourCell((suit * 6 + phase), color);
                if (!pseudo && phase == canonical && phase != 0)
                {
                    RgbColor expected = SamusPaletteFade.EighthTowardWhite(source[suit][0][color], phase);
                    if (value != expected) fadeInputs.Add(key, new(value, expected));
                }
                else colors.Add(keyCell, value);
            }
        }

        /// <summary>Restores one edited or shared ink through canonical phase, suit alias, and calculated tint relationships.</summary>
        internal RgbColor Resolve(int suit, int phase, int color)
        {
            if ((uint)suit >= SamusChargeColorFormat.SuitCount) throw new ArgumentOutOfRangeException(nameof(suit));
            int canonical = SamusChargeColorFormat.CanonicalPhase(pseudo, phase);
            if ((uint)color >= SamusChargeColorFormat.ColorsPerPalette) throw new ArgumentOutOfRangeException(nameof(color));
            int key = (suit * 6 + phase) * 16 + color;
            var keyCell = new ColourCell((suit * 6 + phase), color);
            if (colors.TryGetValue(keyCell, out RgbColor value)) return value;
            if (phase != canonical) return Resolve(suit, canonical, color);
            if (pseudo && phase == 0)
            {
                RgbColor bright = color == 0 ? Resolve(0, 3, 0) :
                    SamusChargeColorFormat.PseudoScrewBrightColor(suit, color, Resolve(suit, 3, color));
                return fadeInputs.TryGetValue(key, out var brightInput) ? brightInput.Apply(bright) : bright;
            }
            if (suit != 0 && (pseudo || phase == 0)) return Resolve(0, phase, color);
            RgbColor expected = SamusPaletteFade.EighthTowardWhite(Resolve(suit, 0, color), phase);
            return fadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
        }
    }
}
