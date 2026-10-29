using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Samus's charge colours (#1464): the suit colours the charged-beam pulse and pseudo Screw Attack show for each suit and
/// step, and the Hyper-shot glow's cycle. The import pipeline decodes them from the installed document; charge admission,
/// step advancement and the glow's timer are Samus's.
/// </summary>
public sealed class SamusChargeColours
{
    /// <summary>Each charge palette's suit colours.</summary>
    private readonly Dictionary<SamusChargePalette, ImmutableArray<RgbColor>> palettes;

    /// <summary>The Hyper-shot glow's colours in full-body cycle order, which its playback walks in reverse.</summary>
    private readonly SamusHyperBeamColours hyperShot;

    /// <summary>Keeps the decoded palettes and the Hyper-shot cycle.</summary>
    /// <param name="palettes">Each charge palette's suit colours, as many as Samus's suit holds.</param>
    /// <param name="hyperShot">The Hyper-shot glow's colours in full-body cycle order.</param>
    /// <exception cref="ArgumentException">A palette does not hold a whole suit's colours.</exception>
    internal SamusChargeColours(IReadOnlyDictionary<SamusChargePalette, ImmutableArray<RgbColor>> palettes,
        SamusHyperBeamColours hyperShot)
    {
        Ensure.NotNull(palettes);
        foreach (ImmutableArray<RgbColor> colours in palettes.Values)
            Ensure.LengthEqual(colours.AsSpan(), SamusColourDefinitions.RowColours);
        this.palettes = new(palettes);
        this.hyperShot = Ensure.NotNull(hyperShot);
    }

    /// <summary>One colour of Samus's suit under a charge palette.</summary>
    /// <param name="palette">The charge palette.</param>
    /// <param name="color">The colour within Samus's suit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette is not catalogued, or the colour is not one of the suit's.</exception>
    internal RgbColor Resolve(SamusChargePalette palette, int color)
    {
        if (!palettes.TryGetValue(palette, out ImmutableArray<RgbColor> colours))
            throw new ArgumentOutOfRangeException(nameof(palette), $"Uncatalogued charge palette {palette}.");
        if ((uint)color >= (uint)colours.Length) throw new ArgumentOutOfRangeException(nameof(color));
        return colours[color];
    }

    /// <summary>One colour of Samus's suit in Hyper-shot playback frame <paramref name="frame"/>.</summary>
    /// <param name="frame">The playback frame, which walks the full-body Hyper Beam cycle in reverse.</param>
    /// <param name="color">The colour within Samus's suit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the cycle.</exception>
    public RgbColor ResolveHyper(int frame, int color)
    {
        int frames = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount;
        if ((uint)frame >= (uint)frames) throw new ArgumentOutOfRangeException(nameof(frame));
        return hyperShot.Resolve(frames - 1 - frame, color);
    }

    /// <summary>Shows a charge palette's colours as Samus's suit colours; charge admission and step advancement remain Samus's.</summary>
    /// <param name="colours">Samus's colours, whose suit colours change.</param>
    /// <param name="pseudo">True for pseudo Screw Attack's bright and normal holds; false for the charged-beam whitening pulse.</param>
    /// <param name="suit">The suit Samus shows.</param>
    /// <param name="phase">The sequence's step.</param>
    public void ApplyCharge(SamusColours colours, bool pseudo, SamusSuit suit, int phase)
    {
        Ensure.NotNull(colours);
        var palette = new SamusChargePalette(pseudo ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam, suit, phase);
        for (int color = 0; color < SamusColourDefinitions.RowColours; color++)
            colours.Suit.Write(color, Resolve(palette, color));
    }

    /// <summary>Shows Hyper-shot playback frame <paramref name="frame"/>'s colours as Samus's suit colours; the glow's timer remains Samus's.</summary>
    /// <param name="colours">Samus's colours, whose suit colours change.</param>
    /// <param name="frame">The playback frame.</param>
    public void ApplyHyper(SamusColours colours, int frame)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < SamusColourDefinitions.RowColours; color++)
            colours.Suit.Write(color, ResolveHyper(frame, color));
    }
}
