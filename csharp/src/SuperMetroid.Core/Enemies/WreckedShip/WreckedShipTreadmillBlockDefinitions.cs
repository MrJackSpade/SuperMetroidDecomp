using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship entrance treadmill's row of blocks: where it lies and how its blocks collide. What they show is the
/// row's installed picture (<see cref="Rooms.RoomObjects.RoomObjectSetupBlock.TreadmillIdle"/>,
/// <see cref="Rooms.RoomObjects.RoomObjectSetupBlock.TreadmillRunning"/>, #1580).
/// </summary>
internal static class WreckedShipTreadmillBlockDefinitions
{
    /// <summary>The treadmill row's first block column, the same in both doors' setup.</summary>
    public const int BlockX = 4;

    /// <summary>The treadmill row's block row, the same in both doors' setup.</summary>
    public const int BlockY = 9;

    /// <summary>The consecutive blocks in the row that setup and activation write.</summary>
    public const int BlockCount = 56;

    /// <summary>How the row's blocks collide while Phantoon lives: as air.</summary>
    public const RoomCollisionType IdleCollision = RoomCollisionType.Air;

    /// <summary>How the row's blocks collide once Phantoon is dead and the belt runs: as special air.</summary>
    public const RoomCollisionType RunningCollision = RoomCollisionType.SpecialAir;

    /// <summary>The block behaviour that carries Samus rightwards.</summary>
    public static readonly RoomBlockBehavior RightwardsBehavior = RoomBlockBehavior.Reacting(SpecialAirReaction.RightwardConveyorOnFoot);

    /// <summary>The block behaviour that carries Samus leftwards.</summary>
    public static readonly RoomBlockBehavior LeftwardsBehavior = RoomBlockBehavior.Reacting(SpecialAirReaction.LeftwardConveyorOnFoot);
}
