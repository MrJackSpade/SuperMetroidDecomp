using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Decoding and component predicates for <see cref="SamusBeamCombination"/>.</summary>
public static class SamusBeamCombinations
{
    /// <summary>The combination at a four-bit table index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside $0-$F.</exception>
    public static SamusBeamCombination FromTableIndex(int index) => (uint)index <= 15
        ? (SamusBeamCombination)index
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Beam-combination table index is outside $0-$F.");

    /// <summary>Exposes the native table index and equipment-bit predicates for a beam combination.</summary>
    extension(SamusBeamCombination combination)
    {
        /// <summary>The four-bit index the native tables use.</summary>
        public int TableIndex => (int)combination;

        /// <summary>The beam components as equipment bits.</summary>
        public SamusBeamFlags Components => (SamusBeamFlags)(byte)combination;

        /// <summary>Whether Wave is a component.</summary>
        public bool HasWave => (combination.Components & SamusBeamFlags.Wave) != SamusBeamFlags.None;

        /// <summary>Whether Ice is a component.</summary>
        public bool HasIce => (combination.Components & SamusBeamFlags.Ice) != SamusBeamFlags.None;

        /// <summary>Whether Spazer is a component.</summary>
        public bool HasSpazer => (combination.Components & SamusBeamFlags.Spazer) != SamusBeamFlags.None;

        /// <summary>Whether Plasma is a component.</summary>
        public bool HasPlasma => (combination.Components & SamusBeamFlags.Plasma) != SamusBeamFlags.None;

        /// <summary>Whether retail equipment can produce this combination: every row the twelve-entry native tables define.</summary>
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
