using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
internal static class SamusSourceDefinitionsConstants2
{
    /// <summary>
    /// $9B:A4B3: the uncharged trail selector table. Each beam combination's row is a bank-$9B pointer to a direction table
    /// of placement-list pointers; rows twelve to fifteen read the charged table's first rows.
    /// </summary>
    internal static readonly SnesAddress UnchargedTrailSelectors = new(0x9b, 0xa4b3);
    /// <summary>$9B:A4CB: the charged trail selector table, laid out as the uncharged one.</summary>
    internal static readonly SnesAddress ChargedTrailSelectors = new(0x9b, 0xa4cb);
    /// <summary>$9B:A4E3: the special beam attack trail selector table, laid out as the uncharged one.</summary>
    internal static readonly SnesAddress SpecialBeamAttackTrailSelectors = new(0x9b, 0xa4e3);
}
