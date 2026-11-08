using System.Collections.Frozen;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The order the cartridge's beam tables index beam combinations by: a combination's place is its components' equipment
/// bits, Wave lowest, then Ice, Spazer and Plasma. Only the import pipeline reads the tables, so only it knows the order.
/// </summary>
internal static class SamusBeamCombinationTableOrder
{
    /// <summary>The combinations in the order the beam tables index them.</summary>
    private static readonly SamusBeamCombination[] TableOrder =
    [
        SamusBeamCombination.Power, SamusBeamCombination.Wave, SamusBeamCombination.Ice, SamusBeamCombination.IceWave,
        SamusBeamCombination.Spazer, SamusBeamCombination.SpazerWave, SamusBeamCombination.SpazerIce,
        SamusBeamCombination.SpazerIceWave, SamusBeamCombination.Plasma, SamusBeamCombination.PlasmaWave,
        SamusBeamCombination.PlasmaIce, SamusBeamCombination.PlasmaIceWave, SamusBeamCombination.SpazerPlasma,
        SamusBeamCombination.SpazerPlasmaWave, SamusBeamCombination.SpazerPlasmaIce,
        SamusBeamCombination.SpazerPlasmaIceWave,
    ];

    /// <summary>Each combination's position in <see cref="TableOrder"/>.</summary>
    private static readonly FrozenDictionary<SamusBeamCombination, int> TablePosition =
        Enumerable.Range(0, TableOrder.Length).ToFrozenDictionary(index => TableOrder[index]);

    /// <summary>The combination at a four-bit table index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside $0-$F.</exception>
    internal static SamusBeamCombination FromTableIndex(int index) => (uint)index < TableOrder.Length
        ? TableOrder[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Beam-combination table index is outside $0-$F.");

    /// <summary>Exposes the beam-table index of a beam combination.</summary>
    extension(SamusBeamCombination combination)
    {
        /// <summary>The four-bit index the beam tables use.</summary>
        internal int TableIndex => TablePosition.TryGetValue(combination, out int index)
            ? index
            : throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination.");
    }
}
