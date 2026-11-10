using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed Samus body colours during beam charge, pseudo-Screw, and Hyper-shot glow, every suit's phase rows
/// and every Hyper-shot frame kept as installed.
/// </summary>
public sealed class InstalledSamusChargeColors
{
    /// <summary>The installed charged-beam rows, by suit, phase and colour.</summary>
    private readonly RgbColor[][][] chargedBeam;
    /// <summary>The installed pseudo-Screw rows, by suit, phase and colour.</summary>
    private readonly RgbColor[][][] pseudoScrew;
    /// <summary>Shot-glow playback uses its own Hyper colours, independent of the full-body cycle asset.</summary>
    private readonly InstalledSamusHyperBeamColors hyperShot;

    /// <summary>Keeps copies of the two complete six-phase suit views with the separately installed Hyper-shot view.</summary>
    /// <param name="chargedBeam">Three suits of six charged-beam phase rows.</param>
    /// <param name="pseudoScrew">Three suits of six pseudo-Screw phase rows.</param>
    /// <param name="hyperShot">The Hyper-shot rows in full-body cycle order.</param>
    /// <exception cref="InvalidDataException">A suit, phase or colour is missing.</exception>
    internal InstalledSamusChargeColors(RgbColor[][][] chargedBeam,
        RgbColor[][][] pseudoScrew, InstalledSamusHyperBeamColors hyperShot)
    {
        this.chargedBeam = Complete(chargedBeam, "charged beam");
        this.pseudoScrew = Complete(pseudoScrew, "pseudo-Screw");
        this.hyperShot = hyperShot;
    }

    /// <summary>
    /// The installed colours keyed by what they show (#1464): every charge palette's suit colours, by sequence, suit and
    /// step, and the Hyper-shot cycle. The document's suit-then-step row order ends here.
    /// </summary>
    public SamusChargeColours ToColours()
    {
        var palettes = new Dictionary<SamusChargePalette, ImmutableArray<RgbColor>>();
        foreach (SamusSuit suit in SamusSuitDefinitions.ListOrder)
        for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
        {
            palettes.Add(new(SamusChargeKind.ChargedBeam, suit, phase), [.. chargedBeam[suit.ListPlace()][phase]]);
            palettes.Add(new(SamusChargeKind.PseudoScrew, suit, phase), [.. pseudoScrew[suit.ListPlace()][phase]]);
        }
        return new(palettes, hyperShot.ToColours());
    }

    /// <summary>Copies <paramref name="rows"/>, rejecting a set without every suit, phase and colour.</summary>
    private static RgbColor[][][] Complete(RgbColor[][][] rows, string name)
    {
        if (rows.Length != SamusChargeColorFormat.SuitCount)
            throw new InvalidDataException($"{name} requires {SamusChargeColorFormat.SuitCount} suits.");
        var copy = new RgbColor[rows.Length][][];
        for (int suit = 0; suit < rows.Length; suit++)
        {
            if (rows[suit].Length != SamusChargeColorFormat.PhasesPerSuit)
                throw new InvalidDataException($"{name} suit {suit} requires {SamusChargeColorFormat.PhasesPerSuit} phases.");
            copy[suit] = new RgbColor[rows[suit].Length][];
            for (int phase = 0; phase < rows[suit].Length; phase++)
            {
                if (rows[suit][phase].Length != SamusChargeColorFormat.ColorsPerPalette)
                    throw new InvalidDataException($"{name} suit {suit}, phase {phase} requires {SamusChargeColorFormat.ColorsPerPalette} colors.");
                copy[suit][phase] = (RgbColor[])rows[suit][phase].Clone();
            }
        }
        return copy;
    }
}
