namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Tourian room-header region, in native table order.</summary>
internal static class TourianRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape4), [
            new(EnemyDefinitionId.PirateSilverWall, PaletteRow: 1),
            new(EnemyDefinitionId.PirateSilverWalking, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MotherBrainSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids10), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DustTorizo), [
            new(EnemyDefinitionId.CorpseTorizo, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BigBoy), [
            new(EnemyDefinitionId.CorpseSidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.CorpseSidehopper2, PaletteRow: 1),
            new(EnemyDefinitionId.BabyMetroid, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MotherBrain01), [
            new(EnemyDefinitionId.MotherBrainHead, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
            new(EnemyDefinitionId.MotherBrainBody, PaletteRow: 1, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEyeDoor), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.BlueHopper), [
            new(EnemyDefinitionId.SidehopperTourian, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape1), [
            new(EnemyDefinitionId.ShutterDestroyable, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianFirst), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids20), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids40), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids11), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids21), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape2), [
            new(EnemyDefinitionId.PirateSilverWall, PaletteRow: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids31), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids41), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianEscape3), [
            new(EnemyDefinitionId.PirateSilverWalking, PaletteRow: 1),
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Seaweed), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.TourianRecharge), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.UpperTourianSave), [
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Metroids30), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.RinkaShaft), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
    ];
}
