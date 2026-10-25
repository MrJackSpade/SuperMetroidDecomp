using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The room blocks, block behaviours and heights the Chozo statues' hands, slopes and acid use.</summary>
internal static class ChozoStatueRoomDefinitions
{
    /// <summary>The base height the Wrecked Ship statue's draining acid settles at, in room pixels.</summary>
    public const ushort LoweredAcidY = 722;

    /// <summary>The first of the two slope blocks the Wrecked Ship statue opens and closes, as a block index in its room.</summary>
    public const int FirstSlopeBlockIndex = 2820;

    /// <summary>The slope shape written at the first slope block.</summary>
    public const byte FirstSlopeBts = 0x12;

    /// <summary>The slope shape written at the adjacent slope block.</summary>
    public const byte SecondSlopeBts = 0x13;

    /// <summary>The special-solid behaviour of the Wrecked Ship hand's block in that area's behaviour table.</summary>
    public static readonly RoomBlockBehavior WreckedShipHandBts = new(0x80);

    /// <summary>The special-solid behaviour of the Lower Norfair hand's block in Norfair's behaviour table.</summary>
    public static readonly RoomBlockBehavior LowerNorfairHandBts = new(0x83);

    /// <summary>
    /// The level word the room-load crumbling plug writes: its setup never loads a value, so it writes what the room
    /// loader's last draw left behind, where the statue-spawned plug writes zero.
    /// </summary>
    public const ushort HardcodedCrumbleSetupLevelWord = 0x0da0;

    /// <summary>The column of the block whose blank air triggers the lowered Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerX = 4;

    /// <summary>The row of the block whose blank air triggers the lowered Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerY = 8;

    /// <summary>The complete blank-air level word the trigger requires, not only an air collision type.</summary>
    public const ushort BlankAirLevelWord = 0x00ff;

    /// <summary>The Wrecked Ship hand's trigger block, where the hand PLM is also placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipHandBlock = new(0x4a, 0x17);

    /// <summary>The block where the Wrecked Ship statue's slope-access PLMs are placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipSlopeAccessBlock = new(0x17, 0x1d);

    /// <summary>The block where the Lower Norfair hand and room-plug PLMs are placed.</summary>
    public static readonly Position16<RoomBlocks> LowerNorfairHandBlock = new(0x0c, 0x1d);
}
