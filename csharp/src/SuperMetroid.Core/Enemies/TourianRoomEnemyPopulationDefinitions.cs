using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.SpacePirates;

using SuperMetroid.Core.Enemies.Shutters;

namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy populations of the Tourian room-header region, in the original list order.</summary>
internal static class TourianRoomEnemyPopulationDefinitions
{
    /// <summary>The region's populations, in the original list order.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape4),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 1232), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 1064), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 776), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 632), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 488), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 344), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWalking, new(624, 384), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(464, 384), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(456, 720), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(320, 512), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(440, 944), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(360, 1280), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(632, 1104), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
        ], 13),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrainSave),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids10),
        [
            new(EnemyDefinitionId.Metroid, new(268, 106), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(637, 116), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(877, 146), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1197, 140), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 4),
        new(nameof(RoomEnemyPopulationDefinitions.DustTorizo),
        [
            new(EnemyDefinitionId.CorpseTorizo, new(288, 148), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, EnemySpawnSettings.None),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BigBoy),
        [
            new(EnemyDefinitionId.BabyMetroid, new(384, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CorpseSidehopper, new(552, 160), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 0)),
            new(EnemyDefinitionId.CorpseSidehopper, new(160, 64), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 1)),
            new(EnemyDefinitionId.CorpseZoomer, new(240, 64), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 0)),
            new(EnemyDefinitionId.CorpseZoomer, new(664, 200), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 1)),
            new(EnemyDefinitionId.CorpseZoomer, new(264, 54), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 2)),
            new(EnemyDefinitionId.CorpseRipper, new(800, 168), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 0)),
            new(EnemyDefinitionId.CorpseRipper, new(194, 200), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 1)),
            new(EnemyDefinitionId.CorpseSkree, new(896, 71), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 0)),
            new(EnemyDefinitionId.CorpseSkree, new(608, 71), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 1)),
            new(EnemyDefinitionId.CorpseSkree, new(384, 71), EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Variant: 2)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrain01),
        [
            new(EnemyDefinitionId.MotherBrainBody, new(129, 111), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, EnemySpawnSettings.None),
            new(EnemyDefinitionId.MotherBrainHead, new(129, 111), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, EnemySpawnSettings.None),
            new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new ZebetiteSpawnSettings(Half: ZebetiteHalf.Primary)),
            new(EnemyDefinitionId.Rinka, new(823, 54), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: true)),
            new(EnemyDefinitionId.Rinka, new(823, 166), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: true)),
            new(EnemyDefinitionId.Rinka, new(631, 28), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: true)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEyeDoor),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BlueHopper),
        [
            new(EnemyDefinitionId.SidehopperTourian, new(248, 97), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new HopperSpawnSettings(UpsideDown: true)),
            new(EnemyDefinitionId.SidehopperTourian, new(134, 169), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new HopperSpawnSettings(UpsideDown: false)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape1),
        [
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 16, FirstRestUnits: 0, SecondRestUnits: 255)),
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 16, FirstRestUnits: 255, SecondRestUnits: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 16, FirstRestUnits: 0, SecondRestUnits: 255)),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 16, FirstRestUnits: 255, SecondRestUnits: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 24, FirstRestUnits: 0, SecondRestUnits: 255)),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 24, FirstRestUnits: 255, SecondRestUnits: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 32, FirstRestUnits: 0, SecondRestUnits: 255)),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(SpeedIndex: 8, PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityOrWaitTime: 32, FirstRestUnits: 255, SecondRestUnits: 0)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianFirst),
        [
            new(EnemyDefinitionId.Elevator, new(128, 706), EnemyProperties.IgnoreSamusCollision | EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new ElevatorSpawnSettings(DirectionIndex: 1, ArrivalY: 24)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids20),
        [
            new(EnemyDefinitionId.Metroid, new(152, 277), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Metroid, new(86, 394), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids40),
        [
            new(EnemyDefinitionId.Metroid, new(196, 338), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(62, 251), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Metroid, new(70, 446), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids11),
        [
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids21),
        [
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape2),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 216), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.None, JumpSpan: 160)),
            new(EnemyDefinitionId.PirateSilverWall, new(208, 296), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateParameterFlags.StartsOnRightWall, JumpSpan: 160)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids31),
        [
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids41),
        [
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape3),
        [
            new(EnemyDefinitionId.PirateSilverWalking, new(192, 352), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1336, 352), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1160, 352), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(984, 352), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1144, 80), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1248, 80), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1376, 80), EnemyProperties.ProcessInstructions, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateParameterFlags.None, PatrolReach: 16)),
        ], 7),
        new(nameof(RoomEnemyPopulationDefinitions.Seaweed),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianRecharge),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.UpperTourianSave),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids30),
        [
            new(EnemyDefinitionId.Metroid, new(394, 129), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(871, 119), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1130, 115), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.RinkaShaft),
        [
            new(EnemyDefinitionId.Rinka, new(232, 440), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(232, 520), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
            new(EnemyDefinitionId.Rinka, new(8, 584), EnemyProperties.ProcessInstructions | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Special: false)),
        ], 0),
    ];
}
