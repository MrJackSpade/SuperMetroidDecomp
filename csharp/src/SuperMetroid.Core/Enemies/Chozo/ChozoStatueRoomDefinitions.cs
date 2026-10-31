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
    public static readonly RoomBlockBehavior FirstSlopeBehavior = new(18);

    /// <summary>The slope shape written at the adjacent slope block.</summary>
    public static readonly RoomBlockBehavior SecondSlopeBehavior = new(19);

    /// <summary>The special-solid behaviour of the Wrecked Ship hand's block in that area's behaviour table.</summary>
    public static readonly RoomBlockBehavior WreckedShipHandBehavior = new(128);

    /// <summary>The special-solid behaviour of the Lower Norfair hand's block in Norfair's behaviour table.</summary>
    public static readonly RoomBlockBehavior LowerNorfairHandBehavior = new(131);

    /// <summary>
    /// The block the room-load crumbling plug writes: its setup never chooses one, so it writes what the room loader's
    /// last draw left behind, block $1A0 mirrored both ways, as air.
    /// </summary>
    public static readonly RoomTerrainBlock HardcodedCrumbleSetupBlock =
        RoomTerrainBlock.Of(416, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air);

    /// <summary>The block the statue-spawned crumbling plug writes: block zero, unmirrored, as air.</summary>
    public static readonly RoomTerrainBlock SpawnedCrumbleSetupBlock =
        RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);

    /// <summary>The column of the block whose blank air triggers the lowered Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerX = 4;

    /// <summary>The row of the block whose blank air triggers the lowered Lower Norfair hand.</summary>
    public const int LowerNorfairTriggerY = 8;

    /// <summary>The Wrecked Ship hand's trigger block, where the hand PLM is also placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipHandBlock = new(74, 23);

    /// <summary>The block where the Wrecked Ship statue's slope-access PLMs are placed.</summary>
    public static readonly Position16<RoomBlocks> WreckedShipSlopeAccessBlock = new(23, 29);

    /// <summary>The block where the Lower Norfair hand and room-plug PLMs are placed.</summary>
    public static readonly Position16<RoomBlocks> LowerNorfairHandBlock = new(12, 29);
}
