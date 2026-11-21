using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Wrecked Ship room-header region, in table order.</summary>
internal static class WreckedShipRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.SpongeBathWhenAreaBossDead, [
            new(EnemyDefinitionId.Bull, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEnergyTankWhenAreaBossDead, [
            new(EnemyDefinitionId.SuspensorPlatform, row: 1),
            new(EnemyDefinitionId.Skultera, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftDefault, [
            new(EnemyDefinitionId.Coven, row: 1),
            new(EnemyDefinitionId.KzanTop, row: 2),
            new(EnemyDefinitionId.Atomic, row: 3),
            new(EnemyDefinitionId.Sbug, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BowlingAlleyDefault, [
            new(EnemyDefinitionId.Chozo, row: 1),
            new(EnemyDefinitionId.Sbug, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AtticWhenAreaBossDead, [
            new(EnemyDefinitionId.KihunterYellow, row: 1),
            new(EnemyDefinitionId.Atomic, row: 2),
            new(EnemyDefinitionId.Spark, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BasementWhenAreaBossDead, [
            new(EnemyDefinitionId.WorkRobot, row: 1),
            new(EnemyDefinitionId.Atomic, row: 2),
            new(EnemyDefinitionId.Spark, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMapWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipWestSuperDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyDeathDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpongeBathDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEntranceWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ElectricDeathDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BowlingAlleyWhenAreaBossDead, [
            new(EnemyDefinitionId.Chozo, row: 1),
            new(EnemyDefinitionId.WorkRobot, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AssemblyLineDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PhantoonWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEnergyTankDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GravitySuitDefault, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMapDefault, [
            new(EnemyDefinitionId.Coven, row: 1),
            new(EnemyDefinitionId.Sbug, row: 7),
            new(EnemyDefinitionId.Bull, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GravitySuitWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BasementDefault, [
            new(EnemyDefinitionId.UnpoweredWorkRobot, row: 7),
            new(EnemyDefinitionId.Sbug, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEntranceDefault, [
            new(EnemyDefinitionId.Sbug, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AssemblyLineWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark, row: 1),
            new(EnemyDefinitionId.Puyo, row: 2),
            new(EnemyDefinitionId.WorkRobot, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipWestSuperWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AtticDefault, [
            new(EnemyDefinitionId.Coven, row: 1),
            new(EnemyDefinitionId.Atomic, row: 2),
            new(EnemyDefinitionId.Sbug, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEastSuperDefault, [
            new(EnemyDefinitionId.Coven, row: 1),
            new(EnemyDefinitionId.Atomic, row: 2),
            new(EnemyDefinitionId.Sbug, row: 2),
            new(EnemyDefinitionId.UnpoweredWorkRobot, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveDefault, [
            new(EnemyDefinitionId.Coven, row: 1),
            new(EnemyDefinitionId.Sbug, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ElectricDeathWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyDeathWhenAreaBossDead, [
            new(EnemyDefinitionId.KzanTop, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEastSuperWhenAreaBossDead, [
            new(EnemyDefinitionId.WorkRobot, row: 1),
            new(EnemyDefinitionId.Bull, row: 2),
            new(EnemyDefinitionId.Spark, row: 3),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PhantoonDefault, [
            new(EnemyDefinitionId.PhantoonBody, row: 7),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark, row: 1),
            new(EnemyDefinitionId.Atomic, row: 2),
            new(EnemyDefinitionId.KzanTop, row: 3),
        ]),
    ];
}
