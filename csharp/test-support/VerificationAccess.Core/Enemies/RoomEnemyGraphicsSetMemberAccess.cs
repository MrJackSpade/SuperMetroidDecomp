using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The oracles' codec for a bank-$B4 graphics-set record's second word: its low byte is the OBJ palette row the member's
/// colours load into, and bits 12 and 13 the fixed staging block, both of which only the import knows (#1512, #1513). The
/// cartridge reads no other bit.
/// </summary>
internal static class RoomEnemyGraphicsSetMemberAccess
{
    /// <summary>The record's palette-row bits.</summary>
    private const int PaletteRowBits = 0x00ff;

    /// <summary>The record's staging-block bits.</summary>
    private const int StagingBlockBits = 0x3000;

    /// <summary>How far the staging block sits above bit zero.</summary>
    private const int StagingBlockShift = 12;

    /// <summary>The last palette row a record names.</summary>
    private const int LastPaletteRow = 7;

    /// <summary>The largest staging block the record's two placement bits select.</summary>
    private const int LastStagingBlock = 3;

    /// <summary>Decodes native records.</summary>
    extension(RoomEnemyGraphicsSetMember)
    {
        /// <summary>The palette row a native record's second word loads the member's colours into, which the import keeps.</summary>
        internal static int NativePaletteRow(ushort placement) => placement & PaletteRowBits;

        /// <summary>The fixed staging block a native record's second word names, which the import keeps.</summary>
        internal static int NativeStagingBlock(ushort placement) => (placement & StagingBlockBits) >> StagingBlockShift;

        /// <summary>
        /// A record's second word, naming palette row <paramref name="paletteRow"/> and fixed staging block
        /// <paramref name="stagingBlock"/>.
        /// </summary>
        internal static ushort NativePlacement(int paletteRow, int stagingBlock) =>
            checked((ushort)(Ensure.BetweenInclusive(0, LastPaletteRow, paletteRow)
                | Ensure.BetweenInclusive(0, LastStagingBlock, stagingBlock) << StagingBlockShift));
    }
}
