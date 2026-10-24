namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Verification's native view of corpse rotting data: the bank-$7E address of a rot table and the bank-$A9 address of
/// a row upload list, as the cartridge's corpse records store them.
/// </summary>
internal static class CorpseNativeAddresses
{
    /// <summary>The bank-$7E address the rotting heap begins at.</summary>
    private const ushort RottingHeap = 0x9000;

    /// <summary>The bank-$7E address of the rot table <paramref name="offset"/> bytes into the rotting heap.</summary>
    internal static ushort RotTableAddress(int offset) => checked((ushort)(RottingHeap + offset));

    /// <summary>The bank-$A9 address of <paramref name="table"/>, from each species' first list and its lists' length.</summary>
    internal static ushort TransferTableAddress(RottingCorpseKind table) => checked((ushort)(table.Species switch
    {
        RottingCorpseSpecies.Sidehopper => 0xe0e0 + 42 * table.Variant,
        RottingCorpseSpecies.Zoomer => 0xe134 + 18 * table.Variant,
        RottingCorpseSpecies.Ripper => 0xe16a + 18 * table.Variant,
        RottingCorpseSpecies.Skree => 0xe18e + 34 * table.Variant,
        RottingCorpseSpecies.Torizo => throw new ArgumentOutOfRangeException(nameof(table), table, "The Torizo corpse has no transfer list."),
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "This species has no corpse transfer lists."),
    }));

    /// <summary>The ten transfer lists, Sidehopper, Zoomer, Ripper then Skree, in native table order.</summary>
    internal static RottingCorpseKind[] AllTransferTables { get; } =
    [
        new(RottingCorpseSpecies.Sidehopper, 0), new(RottingCorpseSpecies.Sidehopper, 1),
        new(RottingCorpseSpecies.Zoomer, 0), new(RottingCorpseSpecies.Zoomer, 1), new(RottingCorpseSpecies.Zoomer, 2),
        new(RottingCorpseSpecies.Ripper, 0), new(RottingCorpseSpecies.Ripper, 1),
        new(RottingCorpseSpecies.Skree, 0), new(RottingCorpseSpecies.Skree, 1), new(RottingCorpseSpecies.Skree, 2),
    ];

    /// <summary>The bank-$A9 transfer list stored at <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No transfer list begins at <paramref name="address"/>.</exception>
    internal static RottingCorpseKind TransferTableAt(ushort address)
    {
        foreach (RottingCorpseKind table in AllTransferTables)
            if (TransferTableAddress(table) == address)
                return table;
        throw new InvalidDataException($"No corpse transfer list begins at $A9:{address:X4}.");
    }

    /// <summary>Native views of the Sidehopper corpse record.</summary>
    extension(DeadSidehopperCorpseDefinition definition)
    {
        /// <summary>The bank-$7E address of its rot table.</summary>
        internal ushort RottingTablePointer => RotTableAddress(definition.RotTableOffset);

        /// <summary>The bank-$A9 address of its row upload list.</summary>
        internal ushort RowUploadPointer => TransferTableAddress(definition.Kind);
    }

    /// <summary>Native views of the dead Torizo corpse record.</summary>
    extension(DeadTorizoCorpseDefinition definition)
    {
        /// <summary>The bank-$7E address of its rot table.</summary>
        internal ushort RottingTablePointer => RotTableAddress(definition.RotTableOffset);
    }

    /// <summary>Native views of a dead Tourian corpse record.</summary>
    extension(DeadTourianCorpseDefinition definition)
    {
        /// <summary>The bank-$7E address of its rot table.</summary>
        internal ushort RottingTablePointer => RotTableAddress(definition.RotTableOffset);

        /// <summary>The bank-$A9 address of its row upload list.</summary>
        internal ushort RowUploadPointer => TransferTableAddress(definition.Kind);
    }
}
