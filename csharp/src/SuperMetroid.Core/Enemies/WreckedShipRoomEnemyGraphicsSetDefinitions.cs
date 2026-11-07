namespace SuperMetroid.Core.Enemies;

/// <summary>The graphics sets of the Wrecked Ship room-header region, in table order.</summary>
internal static class WreckedShipRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpongeBathWhenAreaBossDead), [
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipETankWhenAreaBossDead), [
            new(EnemyDefinitionId.Kamer2),
            new(EnemyDefinitionId.Skultera),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftDefault), [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.KzanTop),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BowlingAlleyDefault), [
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AtticWhenAreaBossDead), [
            new(EnemyDefinitionId.KihunterYellow),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Spark),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BasementWhenAreaBossDead), [
            new(EnemyDefinitionId.Robot),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Spark),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipMapWhenAreaBossDead), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WSWestSuperDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyDeathDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpongeBathDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipEntranceWhenAreaBossDead), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ElectricDeathDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BowlingAlleyWhenAreaBossDead), [
            new(EnemyDefinitionId.Chozo),
            new(EnemyDefinitionId.Robot),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AssemblyLineDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PhantoonWhenAreaBossDead), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipETankDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GravitySuitDefault), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipMapDefault), [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.Bull),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.GravitySuitWhenAreaBossDead), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BasementDefault), [
            new(EnemyDefinitionId.RobotNoPower),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipEntranceDefault), [
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AssemblyLineWhenAreaBossDead), [
            new(EnemyDefinitionId.Spark),
            new(EnemyDefinitionId.Puyo),
            new(EnemyDefinitionId.Robot),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveWhenAreaBossDead), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WSWestSuperWhenAreaBossDead), [
            new(EnemyDefinitionId.Spark),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.AtticDefault), [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WSEastSuperDefault), [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.Sbug),
            new(EnemyDefinitionId.RobotNoPower),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipSaveDefault), [
            new(EnemyDefinitionId.Coven),
            new(EnemyDefinitionId.Sbug),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.ElectricDeathWhenAreaBossDead), [
            new(EnemyDefinitionId.Spark),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.SpikyDeathWhenAreaBossDead), [
            new(EnemyDefinitionId.KzanTop),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WSEastSuperWhenAreaBossDead), [
            new(EnemyDefinitionId.Robot),
            new(EnemyDefinitionId.Bull),
            new(EnemyDefinitionId.Spark),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.PhantoonDefault), [
            new(EnemyDefinitionId.PhantoonBody),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaftWhenAreaBossDead), [
            new(EnemyDefinitionId.Spark),
            new(EnemyDefinitionId.Atomic),
            new(EnemyDefinitionId.KzanTop),
        ]),
    ];
}
