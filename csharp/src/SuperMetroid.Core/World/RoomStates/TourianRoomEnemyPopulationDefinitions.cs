using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The placement lists of the Tourian room-header region, in authored order.</summary>
internal static class TourianRoomEnemyPopulationDefinitions
{
    /// <summary>The region's lists, in authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape4),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 1232), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 1064), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 776), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 632), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 488), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 344), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWalking, new(624, 384), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(464, 384), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(456, 720), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(320, 512), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(440, 944), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(360, 1280), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(632, 1104), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
        ], 13),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrainSave),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids1Default),
        [
            new(EnemyDefinitionId.Metroid, new(268, 106), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(637, 116), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(877, 146), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1197, 140), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 4),
        new(nameof(RoomEnemyPopulationDefinitions.DustTorizo),
        [
            new(EnemyDefinitionId.CorpseTorizo, new(288, 148), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, EnemySpawnSettings.None),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BigBoy),
        [
            new(EnemyDefinitionId.BabyMetroid, new(384, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CorpseSidehopper, new(552, 160), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.ShitroidVictim)),
            new(EnemyDefinitionId.CorpseSidehopper, new(160, 64), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.AlreadyDead)),
            new(EnemyDefinitionId.CorpseZoomer, new(240, 64), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][0])),
            new(EnemyDefinitionId.CorpseZoomer, new(664, 200), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][1])),
            new(EnemyDefinitionId.CorpseZoomer, new(264, 54), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][2])),
            new(EnemyDefinitionId.CorpseRipper, new(800, 168), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Ripper][0])),
            new(EnemyDefinitionId.CorpseRipper, new(194, 200), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Ripper][1])),
            new(EnemyDefinitionId.CorpseSkree, new(896, 71), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][0])),
            new(EnemyDefinitionId.CorpseSkree, new(608, 71), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][1])),
            new(EnemyDefinitionId.CorpseSkree, new(384, 71), EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][2])),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrain01),
        [
            new(EnemyDefinitionId.MotherBrainBody, new(129, 111), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, EnemySpawnSettings.None),
            new(EnemyDefinitionId.MotherBrainHead, new(129, 111), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, EnemySpawnSettings.None),
            new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new ZebetiteSpawnSettings(Half: ZebetiteHalf.Primary)),
            new(EnemyDefinitionId.Rinka, new(823, 54), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
            new(EnemyDefinitionId.Rinka, new(823, 166), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
            new(EnemyDefinitionId.Rinka, new(631, 28), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEyeDoor),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BlueHopper),
        [
            new(EnemyDefinitionId.SidehopperTourian, new(248, 97), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new HopperSpawnSettings(Mounting: HopperMounting.Ceiling)),
            new(EnemyDefinitionId.SidehopperTourian, new(134, 169), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new HopperSpawnSettings(Mounting: HopperMounting.Floor)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape1),
        [
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, FirstRestTime: 0, SecondRestTime: 4080)),
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, FirstRestTime: 4080, SecondRestTime: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, FirstRestTime: 0, SecondRestTime: 4080)),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, FirstRestTime: 4080, SecondRestTime: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, FirstRestTime: 0, SecondRestTime: 4080)),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, FirstRestTime: 4080, SecondRestTime: 0)),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 64), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, FirstRestTime: 0, SecondRestTime: 4080)),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 192), EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, FirstRestTime: 4080, SecondRestTime: 0)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianFirst),
        [
            new(EnemyDefinitionId.Elevator, new(128, 706), EnemyProperties.IgnoreSamusCollision | EnemyProperties.ProcessOffScreen | EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Up, ArrivalY: 24)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids2Default),
        [
            new(EnemyDefinitionId.Metroid, new(152, 277), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Metroid, new(86, 394), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids4Default),
        [
            new(EnemyDefinitionId.Metroid, new(196, 338), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(62, 251), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Metroid, new(70, 446), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids1WhenFirstMetroidHallCleared),
        [
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids2WhenFirstMetroidShaftCleared),
        [
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape2),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 216), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 160)),
            new(EnemyDefinitionId.PirateSilverWall, new(208, 296), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 160)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids3WhenSecondMetroidHallCleared),
        [
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids4WhenSecondMetroidShaftCleared),
        [
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape3),
        [
            new(EnemyDefinitionId.PirateSilverWalking, new(192, 352), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1336, 352), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1160, 352), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(984, 352), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1144, 80), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1248, 80), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1376, 80), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.UsesExtendedSpritemap, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
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
        new(nameof(RoomEnemyPopulationDefinitions.Metroids3Default),
        [
            new(EnemyDefinitionId.Metroid, new(394, 129), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(871, 119), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1130, 115), EnemyProperties.RunsAnimationScript, EnemyExtraProperties.None, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.RinkaShaft),
        [
            new(EnemyDefinitionId.Rinka, new(232, 440), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 520), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 584), EnemyProperties.RunsAnimationScript | EnemyProperties.RespawnIfKilled, EnemyExtraProperties.None, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
    ];
}
