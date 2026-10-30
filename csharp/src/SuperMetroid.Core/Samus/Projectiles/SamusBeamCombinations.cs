using System.Collections.Frozen;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Decoding and component predicates for <see cref="SamusBeamCombination"/>.</summary>
public static class SamusBeamCombinations
{
    /// <summary>
    /// The combinations in the order the native tables index them: a combination's index is its components'
    /// equipment bits, Wave lowest, then Ice, Spazer and Plasma.
    /// </summary>
    private static readonly SamusBeamCombination[] TableOrder =
    [
        SamusBeamCombination.Power, SamusBeamCombination.Wave, SamusBeamCombination.Ice, SamusBeamCombination.IceWave,
        SamusBeamCombination.Spazer, SamusBeamCombination.SpazerWave, SamusBeamCombination.SpazerIce,
        SamusBeamCombination.SpazerIceWave, SamusBeamCombination.Plasma, SamusBeamCombination.PlasmaWave,
        SamusBeamCombination.PlasmaIce, SamusBeamCombination.PlasmaIceWave, SamusBeamCombination.SpazerPlasma,
        SamusBeamCombination.SpazerPlasmaWave, SamusBeamCombination.SpazerPlasmaIce,
        SamusBeamCombination.SpazerPlasmaIceWave,
    ];

    /// <summary>The beam components of each combination, in <see cref="TableOrder"/>.</summary>
    private static readonly SamusBeamFlags[] TableComponents =
    [
        SamusBeamFlags.None, SamusBeamFlags.Wave, SamusBeamFlags.Ice, SamusBeamFlags.Ice | SamusBeamFlags.Wave,
        SamusBeamFlags.Spazer, SamusBeamFlags.Spazer | SamusBeamFlags.Wave, SamusBeamFlags.Spazer | SamusBeamFlags.Ice,
        SamusBeamFlags.Spazer | SamusBeamFlags.Ice | SamusBeamFlags.Wave, SamusBeamFlags.Plasma,
        SamusBeamFlags.Plasma | SamusBeamFlags.Wave, SamusBeamFlags.Plasma | SamusBeamFlags.Ice,
        SamusBeamFlags.Plasma | SamusBeamFlags.Ice | SamusBeamFlags.Wave, SamusBeamFlags.Spazer | SamusBeamFlags.Plasma,
        SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Wave,
        SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Ice,
        SamusBeamFlags.Spazer | SamusBeamFlags.Plasma | SamusBeamFlags.Ice | SamusBeamFlags.Wave,
    ];

    /// <summary>Each combination's position in <see cref="TableOrder"/>.</summary>
    private static readonly FrozenDictionary<SamusBeamCombination, int> TablePosition =
        Enumerable.Range(0, TableOrder.Length).ToFrozenDictionary(index => TableOrder[index]);

    /// <summary>The combination at a four-bit table index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside $0-$F.</exception>
    public static SamusBeamCombination FromTableIndex(int index) => (uint)index < TableOrder.Length
        ? TableOrder[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Beam-combination table index is outside $0-$F.");

    /// <summary>The combination of the beam components in <paramref name="beams"/>; other equipment such as Charge is ignored.</summary>
    public static SamusBeamCombination Of(SamusBeamFlags beams)
    {
        SamusBeamFlags components = beams & (SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma);
        return TableOrder[Array.IndexOf(TableComponents, components)];
    }

    /// <summary>Exposes the native table index and equipment-bit predicates for a beam combination.</summary>
    extension(SamusBeamCombination combination)
    {
        /// <summary>The four-bit index the native tables use.</summary>
        public int TableIndex => TablePosition.TryGetValue(combination, out int index)
            ? index
            : throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination.");

        /// <summary>The beam components.</summary>
        public SamusBeamFlags Components => TableComponents[combination.TableIndex];

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
