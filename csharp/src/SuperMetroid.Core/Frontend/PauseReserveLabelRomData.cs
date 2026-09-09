namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$82 reserve-label data used by pause setup, independent of menu control flow.</summary>
internal static class PauseReserveLabelRomData
{
    /// <summary>$82:C068, kEquipmentTilemapOffs_Tanks: two WRAM destinations.</summary>
    public const int DestinationTable = 0x82c068;
    /// <summary>$82:C088, kEquipmentTilemaps_Tanks: MODE/MANUAL and RESERVE TANK sources.</summary>
    public const int SourceTable = 0x82c088;
    /// <summary>$7E:3800, mutable equipment tilemap base used by those WRAM destinations.</summary>
    public const int TilemapBase = 0x3800;
    /// <summary>$82:A12B copies two labels, each containing seven words.</summary>
    public const int LabelCount = 2;
    /// <summary>$82:A12B copy length for each reserve label.</summary>
    public const int LabelByteCount = 14;
    /// <summary>$82:AB47 writes mode text at equipment word 327.</summary>
    public const int ModeByteOffset = 327 * 2;
    /// <summary>$82:AB47 copies four words for either mode.</summary>
    public const int ModeWordCount = 4;
    /// <summary>$82:BF2A, kEquipmentScreenTilemap_AUTO.</summary>
    public const int AutoTilemap = 0x82bf2a;
    /// <summary>$82:BF22, kEquipmentScreenTilemap_MANUAL.</summary>
    public const int ManualTilemap = 0x82bf22;
    /// <summary>$82:AB47 recognizes reserve-health mode 1 as AUTO; other nonzero modes use MANUAL.</summary>
    public const ushort AutoMode = 1;
    /// <summary>$82:AB47 preserves non-character tilemap bits before OR-ing the authored mode word.</summary>
    public const ushort AttributeMask = 0xfc00;
}
