using System.Collections.Frozen;

namespace SuperMetroid.Core.Progression;

/// <summary>Decoding and component predicates for <see cref="SamusBeamCombination"/>.</summary>
public static class SamusBeamCombinations
{
    /// <summary>The beam components of each combination.</summary>
    private static readonly FrozenDictionary<SamusBeamCombination, SamusBeamFlags> ComponentsOf =
        new Dictionary<SamusBeamCombination, SamusBeamFlags>
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
        }.ToFrozenDictionary();

    /// <summary>The combination each set of beam components forms.</summary>
    private static readonly FrozenDictionary<SamusBeamFlags, SamusBeamCombination> CombinationOf =
        ComponentsOf.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The combination of the beam components in <paramref name="beams"/>; other equipment such as Charge is ignored.</summary>
    public static SamusBeamCombination Of(SamusBeamFlags beams) =>
        CombinationOf[beams & (SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)];

    /// <summary>Exposes the equipment-bit predicates for a beam combination.</summary>
    extension(SamusBeamCombination combination)
    {
        /// <summary>The beam components.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The combination is undefined.</exception>
        public SamusBeamFlags Components => ComponentsOf.TryGetValue(combination, out SamusBeamFlags components)
            ? components
            : throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination.");

        /// <summary>Whether Wave is a component.</summary>
        public bool HasWave => (combination.Components & SamusBeamFlags.Wave) != SamusBeamFlags.None;

        /// <summary>Whether Ice is a component.</summary>
        public bool HasIce => (combination.Components & SamusBeamFlags.Ice) != SamusBeamFlags.None;

        /// <summary>Whether Plasma is a component.</summary>
        public bool HasPlasma => (combination.Components & SamusBeamFlags.Plasma) != SamusBeamFlags.None;

        /// <summary>Whether retail equipment can produce this combination: every one but those pairing Spazer with Plasma.</summary>
        public bool IsRetail => combination switch
        {
            SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
                SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
                SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
                SamusBeamCombination.PlasmaIceWave => true,
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave => false,
            _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
        };
    }
}
