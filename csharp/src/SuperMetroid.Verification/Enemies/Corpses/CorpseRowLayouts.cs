using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// Verification's native view of the corpse row layouts (#1406): the bank-$A9 <c>CorpseRottingTileRowOffsets</c> table a
/// species' corpse configuration names, and the staged byte offset of each of its character rows.
/// </summary>
internal static class CorpseRowLayouts
{
    /// <summary>The bank-$A9 address of <paramref name="species"/>'s row offset table.</summary>
    internal static ushort TableAddress(RottingCorpseSpecies species) => species switch
    {
        RottingCorpseSpecies.Torizo => 0xe226,
        RottingCorpseSpecies.Sidehopper => 0xe240,
        RottingCorpseSpecies.Zoomer => 0xe24c,
        RottingCorpseSpecies.Ripper => 0xe252,
        RottingCorpseSpecies.Skree => 0xe258,
        _ => throw new ArgumentOutOfRangeException(nameof(species), species, "Undefined rotting corpse species."),
    };

    /// <summary>The species whose row offset table lies at bank-$A9 <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No species' table lies there.</exception>
    internal static RottingCorpseSpecies At(ushort address)
    {
        foreach (RottingCorpseSpecies species in Enum.GetValues<RottingCorpseSpecies>())
        {
            if (TableAddress(species) == address)
                return species;
        }
        throw new InvalidDataException($"No corpse row offset table lies at $A9:{address:X4}.");
    }

    /// <summary>The staged byte offset of the character row holding pixel row <paramref name="yOffset"/> in the table at <paramref name="table"/>.</summary>
    internal static ushort RotationOffset(ushort table, ushort yOffset) => RotationOffset(At(table), yOffset);

    /// <summary>The staged byte offset of the character row holding pixel row <paramref name="yOffset"/> of <paramref name="species"/>'s corpse.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row.</exception>
    internal static ushort RotationOffset(RottingCorpseSpecies species, ushort yOffset)
    {
        CorpseRotationExtent extent = CorpseStagingGeometryDefinitions.RotationExtent(species);
        int row = yOffset >> 3;
        if (row >= extent.Count) throw new ArgumentOutOfRangeException(nameof(yOffset));
        return (ushort)(row * extent.Tiles * 32);
    }
}
