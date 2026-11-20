using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Tourian room-header region, in table order.</summary>
internal static class TourianRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every Metroid room set, Metroids and Rinkas, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] MetroidsAndRinkas =
    [
        new(EnemyDefinitionId.Metroid), new(EnemyDefinitionId.Rinka),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape4, [
            new(EnemyDefinitionId.PirateSilverWall),
            new(EnemyDefinitionId.PirateSilverWalking),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MotherBrainSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids1Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.DustTorizo, [
            new(EnemyDefinitionId.CorpseTorizo),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigBoy, [
            new(EnemyDefinitionId.CorpseSidehopper),
            new(UnplacedEnemyArtwork.DeadSidehopperLivingTiles),
            new(EnemyDefinitionId.BabyMetroid),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MotherBrainBeforeDefeat, [
            new(EnemyDefinitionId.MotherBrainHead),
            new(EnemyDefinitionId.Rinka),
            new(EnemyDefinitionId.MotherBrainBody),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueHopper, [
            new(EnemyDefinitionId.SidehopperTourian),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape1, [
            new(EnemyDefinitionId.ShutterDestroyable),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianFirst, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids2Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.Metroids4Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.Metroids1WhenFirstMetroidHallCleared, [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids2WhenFirstMetroidShaftCleared, [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape2, [
            new(EnemyDefinitionId.PirateSilverWall),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids3WhenSecondMetroidHallCleared, [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids4WhenSecondMetroidShaftCleared, [
            new(EnemyDefinitionId.Rinka),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape3, [
            new(EnemyDefinitionId.PirateSilverWalking),
            new(EnemyDefinitionId.ShutterGrowing),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Seaweed, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianRecharge, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.UpperTourianSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids3Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.RinkaShaft, [
            new(EnemyDefinitionId.Rinka),
        ]),
    ];
}
