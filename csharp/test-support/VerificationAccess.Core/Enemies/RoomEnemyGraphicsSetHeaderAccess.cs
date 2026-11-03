namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The oracles' codec for a bank-$B4 graphics-set record's second word: its low byte is the OBJ palette row the member's
/// colours load into, which only the import knows (#1512), and bits 12 and 13 the fixed staging block. The cartridge
/// reads no other bit.
/// </summary>
internal static class RoomEnemyGraphicsSetHeaderAccess
{
    /// <summary>The record's palette-row bits.</summary>
    private const int PaletteRowBits = 0x00ff;

    /// <summary>The record's staging-block bits.</summary>
    private const int StagingBlockBits = 0x3000;

    /// <summary>How far the staging block sits above bit zero.</summary>
    private const int StagingBlockShift = 12;

    /// <summary>The last palette row a record names.</summary>
    private const int LastPaletteRow = 7;

    /// <summary>Decodes native records.</summary>
    extension(RoomEnemyGraphicsSetHeader)
    {
        /// <summary>The member a native record holds, from its definition and its second word; the palette row is the import's.</summary>
        internal static RoomEnemyGraphicsSetHeader FromNative(EnemyDefinitionId definition, ushort placement) =>
            new(definition, (placement & StagingBlockBits) >> StagingBlockShift);

        /// <summary>The palette row a native record's second word loads the member's colours into, which the import keeps.</summary>
        internal static int NativePaletteRow(ushort placement) => placement & PaletteRowBits;
    }

    /// <summary>Encodes a member's native record word.</summary>
    extension(RoomEnemyGraphicsSetHeader self)
    {
        /// <summary>The record's second word, naming palette row <paramref name="paletteRow"/>.</summary>
        internal ushort NativePlacement(int paletteRow) =>
            checked((ushort)(Ensure.BetweenInclusive(0, LastPaletteRow, paletteRow) | self.StagingBlock << StagingBlockShift));
    }
}
