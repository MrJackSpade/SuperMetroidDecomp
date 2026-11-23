using System.Collections.Frozen;

namespace SuperMetroid.Core.Progression;

/// <summary>Decoding and component predicates for <see cref="SamusBeamCombination"/>.</summary>
public static partial class SamusBeamCombinations
{
    /// <summary>The beam components of each combination.</summary>
    private static readonly IdentityTable<SamusBeamCombination, SamusBeamFlags> ComponentsOf = new(new Dictionary<SamusBeamCombination, SamusBeamFlags>
        {
            [SamusBeamCombination.Power] = SamusBeamFlags.None,
            [SamusBeamCombination.Wave] = SamusBeamFlags.Wave,
            [SamusBeamCombination.Ice] = SamusBeamFlags.Ice,
            [SamusBeamCombination.IceWave] = SamusBeamFlags.Ice | SamusBeamFlags.Wave,
            [SamusBeamCombination.Spazer] = SamusBeamFlags.Spazer,
            [SamusBeamCombination.SpazerWave] = SamusBeamFlags.Spazer | SamusBeamFlags.Wave,
            [SamusBeamCombination.SpazerIce] = SamusBeamFlags.Spazer | SamusBeamFlags.Ice,
            [SamusBeamCombination.SpazerIceWave] = SamusBeamFlags.Spazer | SamusBeamFlags.Ice | SamusBeamFlags.Wave,
            [SamusBeamCombination.Plasma] = SamusBeamFlags.Plasma,
            [SamusBeamCombination.PlasmaWave] = SamusBeamFlags.Plasma | SamusBeamFlags.Wave,
            [SamusBeamCombination.PlasmaIce] = SamusBeamFlags.Plasma | SamusBeamFlags.Ice,
            [SamusBeamCombination.PlasmaIceWave] = SamusBeamFlags.Plasma | SamusBeamFlags.Ice | SamusBeamFlags.Wave,
            [SamusBeamCombination.SpazerPlasma] = SamusBeamFlags.Spazer | SamusBeamFlags.Plasma,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Wave,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Ice,
            [SamusBeamCombination.SpazerPlasmaIceWave] =
                SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Ice | SamusBeamFlags.Wave,
        });

    /// <summary>The combination each set of beam components forms.</summary>
    private static readonly FrozenDictionary<SamusBeamFlags, SamusBeamCombination> CombinationOf =
        ComponentsOf.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The combination of the beam components in <paramref name="beams"/>; other equipment such as Charge is ignored.</summary>
    public static SamusBeamCombination Of(SamusBeamFlags beams) => CombinationOf[beams & AllBeams];

    /// <summary>The beam components a combination can hold: Wave, Ice, Spazer and Plasma.</summary>
    private const SamusBeamFlags AllBeams = SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;

    /// <summary>Exposes the equipment predicates for a beam combination.</summary>
    extension(SamusBeamCombination combination)
    {
        /// <summary>The beam components.</summary>
        public SamusBeamFlags Components => ComponentsOf[combination];

        /// <summary>Whether Wave is a component.</summary>
        public bool HasWave => (combination.Components & SamusBeamFlags.Wave) != SamusBeamFlags.None;

        /// <summary>Whether Ice is a component.</summary>
        public bool HasIce => (combination.Components & SamusBeamFlags.Ice) != SamusBeamFlags.None;

        /// <summary>Whether Plasma is a component.</summary>
        public bool HasPlasma => (combination.Components & SamusBeamFlags.Plasma) != SamusBeamFlags.None;

        /// <summary>Whether Spazer is a component.</summary>
        public bool HasSpazer => (combination.Components & SamusBeamFlags.Spazer) != SamusBeamFlags.None;

        /// <summary>Whether Spazer and Plasma are both components, which equipment cannot pair.</summary>
        public bool PairsSpazerWithPlasma => combination.HasSpazer && combination.HasPlasma;

        /// <summary>Whether every beam, Wave, Ice, Spazer and Plasma, is a component.</summary>
        public bool HoldsEveryBeam => combination.Components == AllBeams;

        /// <summary>Whether equipment can produce this combination: every one but those pairing Spazer with Plasma.</summary>
        public bool CanBeEquipped => !combination.PairsSpazerWithPlasma;
    }
}
