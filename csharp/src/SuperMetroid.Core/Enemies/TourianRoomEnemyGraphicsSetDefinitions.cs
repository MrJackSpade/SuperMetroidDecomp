namespace SuperMetroid.Core.Enemies;

/// <summary>The graphics sets of the Tourian room-header region, in table order.</summary>
internal static class TourianRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape4), [
            new(EnemyDefinitionId.PirateSilverWall),
            new(EnemyDefinitionId.PirateSilverWalking),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MotherBrainSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids10), [
            new(EnemyDefinitionId.Metroid),
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DustTorizo), [
            new(EnemyDefinitionId.CorpseTorizo),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigBoy), [
            new(EnemyDefinitionId.CorpseSidehopper),
            new(EnemyDefinitionId.CorpseSidehopper2),
            new(EnemyDefinitionId.BabyMetroid),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MotherBrain01), [
            new(EnemyDefinitionId.MotherBrainHead),
            new(EnemyDefinitionId.Rinka),
            new(EnemyDefinitionId.MotherBrainBody, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEyeDoor), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueHopper), [
            new(EnemyDefinitionId.SidehopperTourian),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape1), [
            new(EnemyDefinitionId.ShutterDestroyable),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianFirst), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids20), [
            new(EnemyDefinitionId.Metroid),
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids40), [
            new(EnemyDefinitionId.Metroid),
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids11), [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids21), [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape2), [
            new(EnemyDefinitionId.PirateSilverWall),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids31), [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids41), [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape3), [
            new(EnemyDefinitionId.PirateSilverWalking),
            new(EnemyDefinitionId.ShutterGrowing),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Seaweed), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianRecharge), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperTourianSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids30), [
            new(EnemyDefinitionId.Metroid),
            new(EnemyDefinitionId.Rinka),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RinkaShaft), [
            new(EnemyDefinitionId.Rinka),
        ]),
    ];
}
