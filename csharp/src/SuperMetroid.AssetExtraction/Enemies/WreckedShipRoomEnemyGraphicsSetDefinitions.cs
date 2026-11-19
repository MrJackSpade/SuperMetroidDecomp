using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Wrecked Ship room-header region, in table order.</summary>
internal static class WreckedShipRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.SpongeBathWhenAreaBossDead, [
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEnergyTankWhenAreaBossDead, [
            new(EnemyDefinitionId.SuspensorPlatform),
            new(EnemyDefinitionId.Skultera),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftDefault, [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.KzanTop),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BowlingAlleyDefault, [
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AtticWhenAreaBossDead, [
            new(EnemyDefinitionId.KihunterYellow),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Spark),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BasementWhenAreaBossDead, [
            new(EnemyDefinitionId.WorkRobot),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Spark),
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
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.WorkRobot),
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
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.Bull),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.GravitySuitWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BasementDefault, [
            new(EnemyDefinitionId.UnpoweredWorkRobot),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEntranceDefault, [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AssemblyLineWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.WorkRobot),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveWhenAreaBossDead, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipWestSuperWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.AtticDefault, [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEastSuperDefault, [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.UnpoweredWorkRobot),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveDefault, [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.ElectricDeathWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.SpikyDeathWhenAreaBossDead, [
            new(EnemyDefinitionId.KzanTop),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipEastSuperWhenAreaBossDead, [
            new(EnemyDefinitionId.WorkRobot),
            new(EnemyDefinitionId.Bull),
            new(EnemyDefinitionId.Spark),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.PhantoonDefault, [
            new(EnemyDefinitionId.PhantoonBody),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftWhenAreaBossDead, [
            new(EnemyDefinitionId.Spark),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.KzanTop),
        ]),
    ];
}
