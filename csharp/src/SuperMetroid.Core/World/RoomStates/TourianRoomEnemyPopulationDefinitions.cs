using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Tourian rooms.</summary>
internal static class TourianRoomEnemyPopulationDefinitions
{
    /// <summary>The Tourian rooms' lists.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape4),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 1232), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 1064), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 776), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 632), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(43, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWall, new(178, 344), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 132)),
            new(EnemyDefinitionId.PirateSilverWalking, new(624, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(464, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(456, 720), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(320, 512), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 32)),
            new(EnemyDefinitionId.PirateSilverWalking, new(440, 944), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(360, 1280), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(632, 1104), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
        ], 13),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrainSave),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids1Default),
        [
            new(EnemyDefinitionId.Metroid, new(268, 106), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(637, 116), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(877, 146), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1197, 140), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 4),
        new(nameof(RoomEnemyPopulationDefinitions.DustTorizo),
        [
            new(EnemyDefinitionId.CorpseTorizo, new(288, 148), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BigBoy),
        [
            new(EnemyDefinitionId.BabyMetroid, new(384, 64), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CorpseSidehopper, new(552, 160), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.ShitroidVictim)),
            new(EnemyDefinitionId.CorpseSidehopper, new(160, 64), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.AlreadyDead)),
            new(EnemyDefinitionId.CorpseZoomer, new(240, 64), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][0])),
            new(EnemyDefinitionId.CorpseZoomer, new(664, 200), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][1])),
            new(EnemyDefinitionId.CorpseZoomer, new(264, 54), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Zoomer][2])),
            new(EnemyDefinitionId.CorpseRipper, new(800, 168), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Ripper][0])),
            new(EnemyDefinitionId.CorpseRipper, new(194, 200), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Ripper][1])),
            new(EnemyDefinitionId.CorpseSkree, new(896, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][0])),
            new(EnemyDefinitionId.CorpseSkree, new(608, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][1])),
            new(EnemyDefinitionId.CorpseSkree, new(384, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Variants[DeadTourianCorpseSpecies.Skree][2])),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MotherBrain01),
        [
            new(EnemyDefinitionId.MotherBrainBody, new(129, 111), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
            new(EnemyDefinitionId.MotherBrainHead, new(129, 111), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
            new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ZebetiteSpawnSettings(Half: ZebetiteHalf.Primary)),
            new(EnemyDefinitionId.Rinka, new(823, 54), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
            new(EnemyDefinitionId.Rinka, new(823, 166), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
            new(EnemyDefinitionId.Rinka, new(631, 28), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.FreeSpawnPoint)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEyeDoor),
        [
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.BlueHopper),
        [
            new(EnemyDefinitionId.SidehopperTourian, new(248, 97), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Mounting: HopperMounting.Ceiling)),
            new(EnemyDefinitionId.SidehopperTourian, new(134, 169), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Mounting: HopperMounting.Floor)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape1),
        [
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
            new(EnemyDefinitionId.ShutterDestroyable, new(440, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
            new(EnemyDefinitionId.ShutterDestroyable, new(360, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
            new(EnemyDefinitionId.ShutterDestroyable, new(280, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.RightOrDown, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
            new(EnemyDefinitionId.ShutterDestroyable, new(200, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), PrimaryDirection: ShutterHeading.LeftOrUp, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianFirst),
        [
            new(EnemyDefinitionId.Elevator, new(128, 706), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Up, ArrivalY: 24)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids2Default),
        [
            new(EnemyDefinitionId.Metroid, new(152, 277), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Metroid, new(86, 394), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids4Default),
        [
            new(EnemyDefinitionId.Metroid, new(196, 338), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(62, 251), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Metroid, new(70, 446), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids1WhenFirstMetroidHallCleared),
        [
            new(EnemyDefinitionId.Rinka, new(40, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(536, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids2WhenFirstMetroidShaftCleared),
        [
            new(EnemyDefinitionId.Rinka, new(152, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(216, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 328), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape2),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(43, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.None, JumpSpan: 160)),
            new(EnemyDefinitionId.PirateSilverWall, new(208, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Options: WallSpacePirateOptions.StartsOnRightWall, JumpSpan: 160)),
        ], 2),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids3WhenSecondMetroidHallCleared),
        [
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Metroids4WhenSecondMetroidShaftCleared),
        [
            new(EnemyDefinitionId.Rinka, new(152, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(152, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(104, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(248, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.TourianEscape3),
        [
            new(EnemyDefinitionId.PirateSilverWalking, new(192, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1336, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1160, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(984, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1144, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1248, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
            new(EnemyDefinitionId.PirateSilverWalking, new(1376, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Options: WalkingSpacePirateOptions.None, PatrolReach: 16)),
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
            new(EnemyDefinitionId.Metroid, new(394, 129), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(871, 119), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Metroid, new(1130, 115), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
            new(EnemyDefinitionId.Rinka, new(264, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(24, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(40, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(440, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(616, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 3),
        new(nameof(RoomEnemyPopulationDefinitions.RinkaShaft),
        [
            new(EnemyDefinitionId.Rinka, new(232, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(232, 520), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
            new(EnemyDefinitionId.Rinka, new(8, 584), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings(Placement: RinkaPlacement.OwnPlacement)),
        ], 0),
    ];
}
