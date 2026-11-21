using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="SamusBeamCombinationTableOrder"/> that only verification reads.</summary>
internal static class SamusBeamCombinationTableOrderConstants
{
    /// <summary>The combination at a four-bit table index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside $0-$F.</exception>
    internal static SamusBeamCombination FromTableIndex(int index) => (uint)index < SamusBeamCombinationTableOrder.TableOrder.Length
        ? SamusBeamCombinationTableOrder.TableOrder[index]
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Beam-combination table index is outside $0-$F.");
}
