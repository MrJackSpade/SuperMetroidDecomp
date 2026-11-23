using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>What each room-object setup block's level words carry (<see cref="RoomObjectSetupBlockFiles"/>).</summary>
internal static partial class RoomObjectSetupBlockDefinitions
{
    /// <summary>A setup that combines the block it substitutes with the struck block's collision, so its word carries none of its own.</summary>
    private static readonly RoomObjectSetupBlockDefinition Substitute = new(RoomCollisionType.Air, KeepsStruckMirroring: false);

    /// <summary>A substituting setup the game shows with the struck block's own mirroring, so its picture names a block only.</summary>
    private static readonly RoomObjectSetupBlockDefinition StruckMirroring = new(RoomCollisionType.Air, KeepsStruckMirroring: true);

    /// <summary>
    /// Each setup block: the treadmill's row words carry the row's own collision; every other setup's word carries none,
    /// and the crumble and Speed Booster blocks keep the struck block's mirroring.
    /// </summary>
    internal static readonly IReadOnlyDictionary<RoomObjectSetupBlock, RoomObjectSetupBlockDefinition> Blocks =
        new Dictionary<RoomObjectSetupBlock, RoomObjectSetupBlockDefinition>
        {
            [RoomObjectSetupBlock.BombBlockParent] = Substitute,
            [RoomObjectSetupBlock.CrumbleBlockParent] = StruckMirroring,
            [RoomObjectSetupBlock.SpeedBoosterBlockParent] = StruckMirroring,
            [RoomObjectSetupBlock.ShotBlockParent] = Substitute,
            [RoomObjectSetupBlock.PowerBombBlockParent] = Substitute,
            [RoomObjectSetupBlock.SuperMissileBlockParent] = Substitute,
            [RoomObjectSetupBlock.CritterWallStruck] = Substitute,
            [RoomObjectSetupBlock.TreadmillIdle] = new(WreckedShipTreadmillBlockDefinitions.IdleCollision, KeepsStruckMirroring: false),
            [RoomObjectSetupBlock.TreadmillRunning] = new(WreckedShipTreadmillBlockDefinitions.RunningCollision, KeepsStruckMirroring: false),
        };
}
