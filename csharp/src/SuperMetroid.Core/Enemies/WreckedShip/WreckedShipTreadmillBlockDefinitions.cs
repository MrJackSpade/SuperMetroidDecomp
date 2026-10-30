using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship entrance treadmill's row of blocks: where it lies and the blocks it writes.</summary>
internal static class WreckedShipTreadmillBlockDefinitions
{
    /// <summary>The treadmill row's first block column, the same in both doors' setup.</summary>
    public const byte BlockX = 4;

    /// <summary>The treadmill row's block row, the same in both doors' setup.</summary>
    public const byte BlockY = 9;

    /// <summary>The consecutive blocks in the row that setup and activation write.</summary>
    public const int BlockCount = 56;

    /// <summary>The block setup writes while Phantoon lives: blank air.</summary>
    public static readonly RoomTerrainBlock InactiveTreadmillBlock = RoomTerrainBlocks.BlankAir;

    /// <summary>The treadmill block written once Phantoon is dead: the blank block, as special air.</summary>
    public static readonly RoomTerrainBlock ActiveTreadmillBlock = RoomTerrainBlock.Of(0xff, BlockMirroring.None, RoomCollisionType.SpecialAir);

    /// <summary>The block behaviour that carries Samus rightwards.</summary>
    public static readonly RoomBlockBehavior RightwardsBehavior = new(0x08);

    /// <summary>The block behaviour that carries Samus leftwards.</summary>
    public static readonly RoomBlockBehavior LeftwardsBehavior = new(0x09);
}
