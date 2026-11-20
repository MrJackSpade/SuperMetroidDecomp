using SuperMetroid.AssetExtraction.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Verification's native view of corpse rotting data: the bank-$A9 address of a row upload list, as the cartridge's
/// corpse records store it.
/// </summary>
internal static class CorpseNativeAddresses
{
    /// <summary>The bank-$A9 address of <paramref name="table"/>, from each species' first list and its lists' length.</summary>
    internal static ushort TransferTableAddress(CorpseRowUploadList table)
    {
        DeadMonsterRowUploadDefinitions.NativeList native = DeadMonsterRowUploadDefinitions.Native(table);
        return checked((ushort)(native.Species switch
        {
            RottingCorpseSpecies.Sidehopper => 0xe0e0 + 42 * native.Variant,
            RottingCorpseSpecies.Zoomer => 0xe134 + 18 * native.Variant,
            RottingCorpseSpecies.Ripper => 0xe16a + 18 * native.Variant,
            RottingCorpseSpecies.Skree => 0xe18e + 34 * native.Variant,
            RottingCorpseSpecies.Torizo => throw new ArgumentOutOfRangeException(nameof(table), table, "This species has no corpse transfer lists."),
            _ => throw new InvalidOperationException($"Undefined {nameof(RottingCorpseSpecies)} {native.Species}."),
        }));
    }

    /// <summary>The ten transfer lists, Sidehopper, Zoomer, Ripper then Skree, in native table order.</summary>
    internal static CorpseRowUploadList[] AllTransferTables { get; } =
    [
        CorpseRowUploadList.SidehopperBabyMetroidVictim, CorpseRowUploadList.SidehopperAlreadyDead,
        CorpseRowUploadList.ZoomerOnItsBack, CorpseRowUploadList.ZoomerUpright, CorpseRowUploadList.SecondZoomerOnItsBack,
        CorpseRowUploadList.RipperLevel, CorpseRowUploadList.RipperTilted,
        CorpseRowUploadList.Skree, CorpseRowUploadList.SecondSkree, CorpseRowUploadList.ThirdSkree,
    ];

    /// <summary>The bank-$A9 transfer list stored at <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No transfer list begins at <paramref name="address"/>.</exception>
    internal static CorpseRowUploadList TransferTableAt(ushort address)
    {
        foreach (CorpseRowUploadList table in AllTransferTables)
            if (TransferTableAddress(table) == address)
                return table;
        throw new InvalidDataException($"No corpse transfer list begins at $A9:{address:X4}.");
    }
}
