using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Tourian room-header region, in table order.</summary>
internal static partial class TourianRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The members of every Metroid room set, Metroids and Rinkas, in record order.</summary>
    private static readonly RoomEnemyGraphicsSetMember[] MetroidsAndRinkas =
    [
        new(EnemyDefinitionId.Metroid, row: 1), new(EnemyDefinitionId.Rinka, row: 2),
    ];

    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape4, [
            new(EnemyDefinitionId.PirateSilverWall, row: 1),
            new(EnemyDefinitionId.PirateSilverWalking, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MotherBrainSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids1Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.DustTorizo, [
            new(EnemyDefinitionId.CorpseTorizo, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BigBoy, [
            new(EnemyDefinitionId.CorpseSidehopper, row: 1),
            new(UnplacedEnemyArtwork.DeadSidehopperLivingTiles, row: 1),
            new(EnemyDefinitionId.BabyMetroid, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MotherBrainBeforeDefeat, [
            new(EnemyDefinitionId.MotherBrainHead, row: 1),
            new(EnemyDefinitionId.Rinka, row: 2),
            new(EnemyDefinitionId.MotherBrainBody, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.BlueHopper, [
            new(EnemyDefinitionId.SidehopperTourian, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape1, [
            new(EnemyDefinitionId.ShutterDestroyable, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianFirst, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids2Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.Metroids4Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.Metroids1WhenFirstMetroidHallCleared, [
            new(EnemyDefinitionId.Rinka, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids2WhenFirstMetroidShaftCleared, [
            new(EnemyDefinitionId.Rinka, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape2, [
            new(EnemyDefinitionId.PirateSilverWall, row: 1),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids3WhenSecondMetroidHallCleared, [
            new(EnemyDefinitionId.Rinka, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids4WhenSecondMetroidShaftCleared, [
            new(EnemyDefinitionId.Rinka, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianEscape3, [
            new(EnemyDefinitionId.PirateSilverWalking, row: 1),
            new(EnemyDefinitionId.ShutterGrowing, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Seaweed, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.TourianRecharge, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.UpperTourianSave, [
        ]),
        new(RoomEnemyGraphicsSetDefinitions.Metroids3Default, MetroidsAndRinkas),
        new(RoomEnemyGraphicsSetDefinitions.RinkaShaft, [
            new(EnemyDefinitionId.Rinka, row: 2),
        ]),
    ];
}
