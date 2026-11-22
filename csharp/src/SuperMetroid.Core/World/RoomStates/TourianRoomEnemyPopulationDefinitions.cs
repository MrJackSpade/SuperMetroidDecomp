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
internal static partial class TourianRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Tourian Escape 4 room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition TourianEscape4 { get; } = new("TourianEscape4",
    [
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(43, 1232)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(178, 1064)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(43, 776)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(178, 632)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(43, 488)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(178, 344)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 132)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(624, 384)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(464, 384)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(456, 720)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(320, 512)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 32)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(440, 944)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(360, 1280)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(632, 1104)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
    ], 13);

    /// <summary>The enemy population of the Mother Brain Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition MotherBrainSave { get; } = new("MotherBrainSave");

    /// <summary>The enemy population of the Metroids 1 room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids1Default { get; } = new("Metroids1Default",
    [
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(268, 106)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(637, 116)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(877, 146)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(1197, 140)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(40, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(536, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1176, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1128, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 4);

    /// <summary>The enemy population of the Dust Torizo room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition DustTorizo { get; } = new("DustTorizo",
    [
        new(EnemyDefinitionId.CorpseTorizo, EnemyPlacing.At(new(288, 148)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Big Boy room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BigBoy { get; } = new("BigBoy",
    [
        new(EnemyDefinitionId.BabyMetroid, EnemyPlacing.At(new(384, 64)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CorpseSidehopper, EnemyPlacing.At(new(552, 160)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.BabyMetroidVictim)),
        new(EnemyDefinitionId.CorpseSidehopper, EnemyPlacing.At(new(160, 64)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new DeadSidehopperSpawnSettings(Layout: DeadSidehopperLayout.AlreadyDead)),
        new(EnemyDefinitionId.CorpseZoomer, EnemyPlacing.At(new(240, 64)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerOnItsBack)),
        new(EnemyDefinitionId.CorpseZoomer, EnemyPlacing.At(new(664, 200)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerUpright)),
        new(EnemyDefinitionId.CorpseZoomer, EnemyPlacing.At(new(264, 54)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.ZoomerOnItsBack)),
        new(EnemyDefinitionId.CorpseRipper, EnemyPlacing.At(new(800, 168)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.RipperLevel)),
        new(EnemyDefinitionId.CorpseRipper, EnemyPlacing.At(new(194, 200)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.RipperTilted)),
        new(EnemyDefinitionId.CorpseSkree, EnemyPlacing.At(new(896, 71)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
        new(EnemyDefinitionId.CorpseSkree, EnemyPlacing.At(new(608, 71)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
        new(EnemyDefinitionId.CorpseSkree, EnemyPlacing.At(new(384, 71)), EnemyContact.SolidToSamus, EnemyProcessing.Animates, EnemyPresence.None, false, new CorpseSpawnSettings(Corpse: DeadTourianCorpseDefinitions.Skree)),
    ], 0);

    /// <summary>The enemy population of the Mother Brain room until she is defeated.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MotherBrainBeforeDefeat { get; } = new("MotherBrainBeforeDefeat",
    [
        new(EnemyDefinitionId.MotherBrainBody, EnemyPlacing.At(new(129, 111)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.MotherBrainHead, EnemyPlacing.At(new(129, 111)), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, true, EnemySpawnSettings.None),
        new(EnemyDefinitionId.Zebetite, EnemyPlacing.BySetup, EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new ZebetiteSpawnSettings(Half: ZebetiteHalf.Primary)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(823, 54)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.RightUpper)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(823, 166)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.RightLower)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(631, 28)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, new RinkaSpawnSettings.SharingPoints(RinkaSpawnPoint.CentreUpper)),
    ], 0);

    /// <summary>The enemy population of the Tourian Eye Door room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition TourianEyeDoor { get; } = new("TourianEyeDoor");

    /// <summary>The enemy population of the Blue Hopper room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition BlueHopper { get; } = new("BlueHopper",
    [
        new(EnemyDefinitionId.SidehopperTourian, EnemyPlacing.At(new(248, 97)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Ceiling)),
        new(EnemyDefinitionId.SidehopperTourian, EnemyPlacing.At(new(134, 169)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new HopperSpawnSettings(Surface: HopperSurface.Floor)),
    ], 2);

    /// <summary>The enemy population of the Tourian Escape 1 room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition TourianEscape1 { get; } = new("TourianEscape1",
    [
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(440, 64)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, Trigger: new ShutterTrigger.Timer(16), TravelDistance: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(440, 192)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(16), TravelDistance: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(360, 64)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, Trigger: new ShutterTrigger.Timer(16), TravelDistance: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(360, 192)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(16), TravelDistance: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(280, 64)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, Trigger: new ShutterTrigger.Timer(24), TravelDistance: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(280, 192)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(24), TravelDistance: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(200, 64)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Down, Trigger: new ShutterTrigger.Timer(32), TravelDistance: 32, RestAfterMovingUp: ShutterRest.For(0), RestAfterMovingDown: ShutterRest.Permanent)),
        new(EnemyDefinitionId.ShutterDestroyable, EnemyPlacing.At(new(200, 192)), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ShutterSpawnSettings(Speed: SignedSixteenSixteen.FromPixelFraction(1, 2), PrimaryDirection: VerticalDirection.Up, Trigger: new ShutterTrigger.Timer(32), TravelDistance: 32, RestAfterMovingUp: ShutterRest.Permanent, RestAfterMovingDown: ShutterRest.For(0))),
    ], 0);

    /// <summary>The enemy population of the Tourian First room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition TourianFirst { get; } = new("TourianFirst",
    [
        new(EnemyDefinitionId.Elevator, EnemyPlacing.At(new(128, 706)), EnemyContact.IgnoresSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new ElevatorSpawnSettings(Direction: VerticalDirection.Up, ArrivalY: 24)),
    ], 0);

    /// <summary>The enemy population of the Metroids 2 room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids2Default { get; } = new("Metroids2Default",
    [
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(152, 277)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(216, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(216, 440)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(8, 328)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(86, 394)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
    ], 2);

    /// <summary>The enemy population of the Metroids 4 room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids4Default { get; } = new("Metroids4Default",
    [
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(196, 338)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(62, 251)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 392)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(104, 296)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(248, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(70, 446)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
    ], 3);

    /// <summary>The enemy population of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids1WhenFirstMetroidHallCleared { get; } = new("Metroids1WhenFirstMetroidHallCleared",
    [
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(40, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(536, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1176, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1128, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = new("Metroids2WhenFirstMetroidShaftCleared",
    [
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(216, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(216, 440)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(8, 328)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Tourian Escape 2 room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition TourianEscape2 { get; } = new("TourianEscape2",
    [
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(43, 216)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
        new(EnemyDefinitionId.PirateSilverWall, EnemyPlacing.At(new(208, 296)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WallSpacePirateSpawnSettings(Wall: HorizontalDirection.Right, LaserSpeed: PirateLaserSpeed.Fast, JumpSpeed: WallSpacePirateJumpSpeed.Fast, JumpSpan: 160)),
    ], 2);

    /// <summary>The enemy population of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids3WhenSecondMetroidHallCleared { get; } = new("Metroids3WhenSecondMetroidHallCleared",
    [
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(264, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(24, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(40, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(232, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(440, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(616, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(616, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1496, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1464, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = new("Metroids4WhenSecondMetroidShaftCleared",
    [
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(152, 392)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(104, 296)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(248, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 0);

    /// <summary>The enemy population of the Tourian Escape 3 room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition TourianEscape3 { get; } = new("TourianEscape3",
    [
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(192, 352)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(1336, 352)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(1160, 352)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(984, 352)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(1144, 80)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(1248, 80)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
        new(EnemyDefinitionId.PirateSilverWalking, EnemyPlacing.At(new(1376, 80)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, true, new WalkingSpacePirateSpawnSettings(Facing: HorizontalDirection.Left, LaserSpeed: PirateLaserSpeed.Fast, ShotWatch: WalkingSpacePirateShotWatch.IgnoresShots, PatrolReach: 16)),
    ], 7);

    /// <summary>The enemy population of the Seaweed room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition Seaweed { get; } = new("Seaweed");

    /// <summary>The enemy population of the Tourian Recharge room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition TourianRecharge { get; } = new("TourianRecharge");

    /// <summary>The enemy population of the Mother Brain room once Tourian's boss is dead: none.</summary>
    internal static EmptyRoomEnemyPopulationDefinition MotherBrainWhenAreaBossDead { get; } = new("MotherBrainWhenAreaBossDead");

    /// <summary>The enemy population of the Upper Tourian Save room.</summary>
    internal static EmptyRoomEnemyPopulationDefinition UpperTourianSave { get; } = new("UpperTourianSave");

    /// <summary>The enemy population of the Metroids 3 room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition Metroids3Default { get; } = new("Metroids3Default",
    [
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(394, 129)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(871, 119)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Metroid, EnemyPlacing.At(new(1130, 115)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new MetroidSpawnSettings(IceDamageToFreeze: 5)),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(264, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(24, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(40, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(232, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(440, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(616, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(616, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1496, 72)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(1464, 184)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
    ], 3);

    /// <summary>The enemy population of the Rinka Shaft room.</summary>
    internal static PlacedRoomEnemyPopulationDefinition RinkaShaft { get; } = new("RinkaShaft",
    [
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(232, 440)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(232, 520)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
        new(EnemyDefinitionId.Rinka, EnemyPlacing.At(new(8, 584)), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.RespawnsIfKilled, false, RinkaSpawnSettings.AtOwnPlacement),
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
        MotherBrainWhenAreaBossDead,
        UpperTourianSave,
        Metroids3Default,
        RinkaShaft,
    ];
}
