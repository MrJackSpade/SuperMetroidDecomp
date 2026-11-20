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
[NumberedIdentities("Each numbered member is the enemy population of a room whose own name carries the number (Grapple Tutorial Room 1-3, Metroid Room 1-4, Tourian Escape Room 1-4); nothing indexes the populations by number.")]
internal static class TourianRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Tourian Escape 4 room.</summary>
    internal static RoomEnemyPopulationDefinition TourianEscape4 { get; } = new("TourianEscape4",
    [
        new(EnemyDefinitionId.PirateSilverWall, new(43, 1232), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, new(178, 1064), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, new(43, 776), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, new(178, 632), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, new(43, 488), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, new(178, 344), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWalking, new(624, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, new(464, 384), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, new(456, 720), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(320, 512), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, new(440, 944), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(360, 1280), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(632, 1104), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
    ], 13);

    /// <summary>The enemy population of the Mother Brain Save room.</summary>
    internal static RoomEnemyPopulationDefinition MotherBrainSave { get; } = new("MotherBrainSave",
    [
    ], 0);

    /// <summary>The enemy population of the Metroids 1 room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition Metroids1Default { get; } = new("Metroids1Default",
    [
        new(EnemyDefinitionId.Metroid, new(268, 106), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(637, 116), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(877, 146), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(1197, 140), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, new(40, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(536, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 4);

    /// <summary>The enemy population of the Dust Torizo room.</summary>
    internal static RoomEnemyPopulationDefinition DustTorizo { get; } = new("DustTorizo",
    [
        new(EnemyDefinitionId.CorpseTorizo, new(288, 148), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Big Boy room.</summary>
    internal static RoomEnemyPopulationDefinition BigBoy { get; } = new("BigBoy",
    [
        new(EnemyDefinitionId.BabyMetroid, new(384, 64), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CorpseSidehopper, new(552, 160), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.BabyMetroidVictim)),
        new(EnemyDefinitionId.CorpseSidehopper, new(160, 64), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.AlreadyDead)),
        new(EnemyDefinitionId.CorpseZoomer, new(240, 64), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerOnItsBack)),
        new(EnemyDefinitionId.CorpseZoomer, new(664, 200), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerUpright)),
        new(EnemyDefinitionId.CorpseZoomer, new(264, 54), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerOnItsBack)),
        new(EnemyDefinitionId.CorpseRipper, new(800, 168), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.RipperLevel)),
        new(EnemyDefinitionId.CorpseRipper, new(194, 200), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.RipperTilted)),
        new(EnemyDefinitionId.CorpseSkree, new(896, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
        new(EnemyDefinitionId.CorpseSkree, new(608, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
        new(EnemyDefinitionId.CorpseSkree, new(384, 71), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
    ], 0);

    /// <summary>The enemy population of the Mother Brain room until she is defeated.</summary>
    internal static RoomEnemyPopulationDefinition MotherBrainBeforeDefeat { get; } = new("MotherBrainBeforeDefeat",
    [
        new(EnemyDefinitionId.MotherBrainBody, new(129, 111), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.MotherBrainHead, new(129, 111), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ZebetiteSpawnSettings(Half: ZebetiteHalf.Primary)), // The cartridge's population entry: the room's placement, whose position the setup replaces with the standing generation's before anything reads it (#2558).
        new(EnemyDefinitionId.Rinka, new(823, 54), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.RightUpper)),
        new(EnemyDefinitionId.Rinka, new(823, 166), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.RightLower)),
        new(EnemyDefinitionId.Rinka, new(631, 28), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.CentreUpper)),
    ], 0);

    /// <summary>The enemy population of the Tourian Eye Door room.</summary>
    internal static RoomEnemyPopulationDefinition TourianEyeDoor { get; } = new("TourianEyeDoor",
    [
    ], 0);

    /// <summary>The enemy population of the Blue Hopper room.</summary>
    internal static RoomEnemyPopulationDefinition BlueHopper { get; } = new("BlueHopper",
    [
        new(EnemyDefinitionId.SidehopperTourian, new(248, 97), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.SidehopperTourian, new(134, 169), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
    ], 2);

    /// <summary>The enemy population of the Tourian Escape 1 room.</summary>
    internal static RoomEnemyPopulationDefinition TourianEscape1 { get; } = new("TourianEscape1",
    [
        new(EnemyDefinitionId.ShutterDestroyable, new(440, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, new(440, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, new(360, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, new(360, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 16, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, new(280, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, new(280, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 24, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, new(200, 64), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, new(200, 192), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, TriggerMode: ShutterTriggerMode.Timer, TravelDistance: 32, ProximityDistance: 0, WaitTime: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
    ], 0);

    /// <summary>The enemy population of the Tourian First room.</summary>
    internal static RoomEnemyPopulationDefinition TourianFirst { get; } = new("TourianFirst",
    [
        new(EnemyDefinitionId.Elevator, new(128, 706), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: ElevatorTravelDirection.Up, ArrivalY: 24)),
    ], 0);

    /// <summary>The enemy population of the Metroids 2 room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition Metroids2Default { get; } = new("Metroids2Default",
    [
        new(EnemyDefinitionId.Metroid, new(152, 277), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, new(152, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(216, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(216, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(8, 328), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Metroid, new(86, 394), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
    ], 2);

    /// <summary>The enemy population of the Metroids 4 room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition Metroids4Default { get; } = new("Metroids4Default",
    [
        new(EnemyDefinitionId.Metroid, new(196, 338), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(62, 251), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, new(152, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(152, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(104, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(248, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Metroid, new(70, 446), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
    ], 3);

    /// <summary>The enemy population of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    internal static RoomEnemyPopulationDefinition Metroids1WhenFirstMetroidHallCleared { get; } = new("Metroids1WhenFirstMetroidHallCleared",
    [
        new(EnemyDefinitionId.Rinka, new(40, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(536, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1176, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1128, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    internal static RoomEnemyPopulationDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = new("Metroids2WhenFirstMetroidShaftCleared",
    [
        new(EnemyDefinitionId.Rinka, new(152, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(216, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(216, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(8, 328), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Tourian Escape 2 room.</summary>
    internal static RoomEnemyPopulationDefinition TourianEscape2 { get; } = new("TourianEscape2",
    [
        new(EnemyDefinitionId.PirateSilverWall, new(43, 216), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateSilverWall, new(208, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
    ], 2);

    /// <summary>The enemy population of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    internal static RoomEnemyPopulationDefinition Metroids3WhenSecondMetroidHallCleared { get; } = new("Metroids3WhenSecondMetroidHallCleared",
    [
        new(EnemyDefinitionId.Rinka, new(264, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(24, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(40, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(232, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(440, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(616, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(616, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    internal static RoomEnemyPopulationDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = new("Metroids4WhenSecondMetroidShaftCleared",
    [
        new(EnemyDefinitionId.Rinka, new(152, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(152, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(104, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(248, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Tourian Escape 3 room.</summary>
    internal static RoomEnemyPopulationDefinition TourianEscape3 { get; } = new("TourianEscape3",
    [
        new(EnemyDefinitionId.PirateSilverWalking, new(192, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(1336, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(1160, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(984, 352), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(1144, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(1248, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, new(1376, 80), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
    ], 7);

    /// <summary>The enemy population of the Seaweed room.</summary>
    internal static RoomEnemyPopulationDefinition Seaweed { get; } = new("Seaweed",
    [
    ], 0);

    /// <summary>The enemy population of the Tourian Recharge room.</summary>
    internal static RoomEnemyPopulationDefinition TourianRecharge { get; } = new("TourianRecharge",
    [
    ], 0);

    /// <summary>The enemy population of the Upper Tourian Save room.</summary>
    internal static RoomEnemyPopulationDefinition UpperTourianSave { get; } = new("UpperTourianSave",
    [
    ], 0);

    /// <summary>The enemy population of the Metroids 3 room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition Metroids3Default { get; } = new("Metroids3Default",
    [
        new(EnemyDefinitionId.Metroid, new(394, 129), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(871, 119), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, new(1130, 115), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, new(264, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(24, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(40, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(232, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(440, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(616, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(616, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1496, 72), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(1464, 184), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 3);

    /// <summary>The enemy population of the Rinka Shaft room.</summary>
    internal static RoomEnemyPopulationDefinition RinkaShaft { get; } = new("RinkaShaft",
    [
        new(EnemyDefinitionId.Rinka, new(232, 440), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(232, 520), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, new(8, 584), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The Tourian rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        TourianEscape4,
        MotherBrainSave,
        Metroids1Default,
        DustTorizo,
        BigBoy,
        MotherBrainBeforeDefeat,
        TourianEyeDoor,
        BlueHopper,
        TourianEscape1,
        TourianFirst,
        Metroids2Default,
        Metroids4Default,
        Metroids1WhenFirstMetroidHallCleared,
        Metroids2WhenFirstMetroidShaftCleared,
        TourianEscape2,
        Metroids3WhenSecondMetroidHallCleared,
        Metroids4WhenSecondMetroidShaftCleared,
        TourianEscape3,
        Seaweed,
        TourianRecharge,
        UpperTourianSave,
        Metroids3Default,
        RinkaShaft,
    ];
}
