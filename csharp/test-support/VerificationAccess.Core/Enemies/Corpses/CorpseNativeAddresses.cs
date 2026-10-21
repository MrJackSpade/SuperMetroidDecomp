namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Verification's native view of corpse rotting data: the bank-$7E address of a rot table and the bank-$A9 address of
/// a VRAM transfer list, as the cartridge's corpse records store them.
/// </summary>
internal static class CorpseNativeAddresses
{
    /// <summary>The bank-$7E address the rotting heap begins at.</summary>
    private const ushort RottingHeap = 0x9000;

    /// <summary>The bank-$7E address of the rot table <paramref name="offset"/> bytes into the rotting heap.</summary>
    internal static ushort RotTableAddress(int offset) => checked((ushort)(RottingHeap + offset));

    /// <summary>The bank-$A9 address of <paramref name="table"/>, from each species' first list and its lists' length.</summary>
    internal static ushort TransferTableAddress(CorpseTransferTable table) => checked((ushort)(table.Species switch
    {
        CorpseRotationTable.Sidehopper => 0xe0e0 + 42 * table.Variant,
        CorpseRotationTable.Zoomer => 0xe134 + 18 * table.Variant,
        CorpseRotationTable.Ripper => 0xe16a + 18 * table.Variant,
        CorpseRotationTable.Skree => 0xe18e + 34 * table.Variant,
        CorpseRotationTable.Torizo => throw new ArgumentOutOfRangeException(nameof(table), table, "The Torizo corpse has no transfer list."),
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "This species has no corpse transfer lists."),
    }));

    /// <summary>The ten transfer lists, Sidehopper, Zoomer, Ripper then Skree, in native table order.</summary>
    internal static CorpseTransferTable[] AllTransferTables { get; } =
    [
        new(CorpseRotationTable.Sidehopper, 0), new(CorpseRotationTable.Sidehopper, 1),
        new(CorpseRotationTable.Zoomer, 0), new(CorpseRotationTable.Zoomer, 1), new(CorpseRotationTable.Zoomer, 2),
        new(CorpseRotationTable.Ripper, 0), new(CorpseRotationTable.Ripper, 1),
        new(CorpseRotationTable.Skree, 0), new(CorpseRotationTable.Skree, 1), new(CorpseRotationTable.Skree, 2),
    ];

    /// <summary>The bank-$A9 transfer list stored at <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No transfer list begins at <paramref name="address"/>.</exception>
    internal static CorpseTransferTable TransferTableAt(ushort address)
    {
        foreach (CorpseTransferTable table in AllTransferTables)
            if (TransferTableAddress(table) == address)
                return table;
        throw new InvalidDataException($"No corpse transfer list begins at $A9:{address:X4}.");
    }

    /// <summary>Transfer lists by their bank-$A9 address.</summary>
    extension(DeadMonsterRottingDefinitions)
    {
        /// <summary>The rows of the transfer list stored at <paramref name="address"/>.</summary>
        internal static System.Collections.Immutable.ImmutableArray<DeadMonsterVramTransferDefinition> ForTransferTable(ushort address) =>
            DeadMonsterRottingDefinitions.ForTransferTable(TransferTableAt(address));
    }

    /// <summary>Native views of the Sidehopper corpse record.</summary>
    extension(DeadSidehopperCorpseDefinition definition)
    {
        /// <summary>The bank-$7E address of its rot table.</summary>
        internal ushort RottingTablePointer => RotTableAddress(definition.RotTableOffset);

        /// <summary>The bank-$A9 address of its VRAM transfer list.</summary>
        internal ushort VramTransferPointer => TransferTableAddress(definition.VramTransfers);
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

        /// <summary>The bank-$A9 address of its VRAM transfer list.</summary>
        internal ushort VramTransferPointer => TransferTableAddress(definition.VramTransfers);
    }
}
