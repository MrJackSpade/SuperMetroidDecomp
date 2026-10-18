namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The oracles' codec for a bank-$B4 graphics-set record's second word: its low byte is the OBJ palette row and bits
/// 12 and 13 the fixed staging block. The cartridge reads no other bit.
/// </summary>
internal static class RoomEnemyGraphicsSetHeaderAccess
{
    /// <summary>The record's palette-row bits.</summary>
    private const int PaletteRowBits = 0x00ff;

    /// <summary>The record's staging-block bits.</summary>
    private const int StagingBlockBits = 0x3000;

    /// <summary>How far the staging block sits above bit zero.</summary>
    private const int StagingBlockShift = 12;

    /// <summary>Decodes native records.</summary>
    extension(RoomEnemyGraphicsSetHeader)
    {
        /// <summary>The member a native record holds, from its definition and its second word.</summary>
        internal static RoomEnemyGraphicsSetHeader FromNative(EnemyDefinitionId definition, ushort placement) =>
            new(definition, placement & PaletteRowBits, (placement & StagingBlockBits) >> StagingBlockShift);
    }

    /// <summary>Encodes a member's native record word.</summary>
    extension(RoomEnemyGraphicsSetHeader self)
    {
        /// <summary>The record's second word.</summary>
        internal ushort NativePlacement => checked((ushort)(self.PaletteRow | self.StagingBlock << StagingBlockShift));
    }
}
