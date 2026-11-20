using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The title demonstrations, each scene's staging written once in its entry (#2500).</summary>
/// <remarks>
/// The demonstrations' order, where each starts, what Samus holds and where each recording ends are the title's choices;
/// where she stands follows from her staging pose, and her ammunition and energy from the tanks each scene gives her.
/// </remarks>
public static class StockAttractDemoScenes
{
    /// <summary>The abilities the Speed Booster's and later demonstrations build on.</summary>
    private const SamusEquipmentFlags Mobility = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
        SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpeedBooster;

    /// <summary><see cref="Mobility"/> with the Grapple Beam and the X-ray Scope.</summary>
    private const SamusEquipmentFlags Exploration = Mobility | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope;

    /// <summary><see cref="Exploration"/> with the Gravity Suit and Space Jump.</summary>
    private const SamusEquipmentFlags SpaceJump = Exploration | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpaceJump;

    /// <summary><see cref="SpaceJump"/> with the Screw Attack.</summary>
    private const SamusEquipmentFlags ScrewAttack = SpaceJump | SamusEquipmentFlags.ScrewAttack;

    /// <summary><see cref="ScrewAttack"/> with bombs: every item but the Spring Ball.</summary>
    private const SamusEquipmentFlags BombAbilities = ScrewAttack | SamusEquipmentFlags.Bombs;

    /// <summary>Every beam.</summary>
    private const SamusBeamFlags AllBeams =
        SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;

    /// <summary>The set the title plays first after power-on or a soft reset.</summary>
    public static AttractDemoSet FirstSet { get; } = new(nameof(FirstSet),
    [
        new(
            Room: RoomDefinitions.LandingSite,
            Door: CrateriaDoorDefinitions.ParlorToLandingSite,
            CameraPosition: CameraAtScreen(new(4, 4)),
            Samus: AttractDemoSamusSetups.LandingSite.At(new(1153, 1088)),
            Duration: 1235,
            RoomSetup: null,
            Items: SamusEquipmentFlags.None,
            Missiles: MissilesForTanks(0),
            SuperMissiles: SuperMissilesForTanks(0),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractLandingSite)),
        new(
            Room: RoomDefinitions.ConstructionZone,
            Door: BrinstarDoorDefinitions.MorphBallToConstructionZone,
            CameraPosition: CameraAtScreen(new(0, 0)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(82, floorRow: 8),
            Duration: 337,
            RoomSetup: null,
            Items: SamusEquipmentFlags.MorphBall,
            Missiles: MissilesForTanks(1),
            SuperMissiles: SuperMissilesForTanks(0),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractMissileDoor)),
        new(
            Room: RoomDefinitions.SporeSpawnKihunters,
            Door: BrinstarDoorDefinitions.BigPinkToSporeSpawnKihunters,
            CameraPosition: CameraAtScreen(new(1, 0)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(352, floorRow: 13),
            Duration: 378,
            RoomSetup: null,
            Items: SamusEquipmentFlags.MorphBall,
            Missiles: MissilesForTanks(3),
            SuperMissiles: SuperMissilesForTanks(0),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractPreSporeSpawnHall)),
        new(
            Room: RoomDefinitions.FrogSpeedway,
            Door: NorfairDoorDefinitions.UpperNorfairFarmingToFrogSpeedway,
            CameraPosition: CameraAtScreen(new(7, 0)),
            Samus: AttractDemoSamusSetups.StandingLeft.OnFloor(1992, floorRow: 10),
            Duration: 420,
            RoomSetup: null,
            Items: Mobility,
            Missiles: MissilesForTanks(6),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractSpeedBooster)),
        new(
            Room: RoomDefinitions.SpikyAcidSnakesTunnel,
            Door: NorfairDoorDefinitions.UpperNorfairRefillToSpikyAcidSnakesTunnel,
            CameraPosition: CameraAtScreen(new(0, 0)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(66, floorRow: 10),
            Duration: 444,
            RoomSetup: null,
            Items: Mobility | SamusEquipmentFlags.GrappleBeam,
            Missiles: MissilesForTanks(6),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractGrappleBeam)),
        new(
            Room: RoomDefinitions.BigPink,
            Door: BrinstarDoorDefinitions.GreenHillZoneToBigPink,
            CameraPosition: CameraAtScreen(new(2, 6)),
            Samus: AttractDemoSamusSetups.MorphLeft.OnFloor(679, floorRow: 106),
            Duration: 613,
            RoomSetup: AttractDemoRoomSetup.ChargeBeamScroll,
            Items: SamusEquipmentFlags.MorphBall,
            Missiles: MissilesForTanks(4),
            SuperMissiles: SuperMissilesForTanks(0),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractPseudoScrewAttack)),
    ]);

    /// <summary>The set the title plays after <see cref="FirstSet"/>.</summary>
    public static AttractDemoSet SecondSet { get; } = new(nameof(SecondSet),
    [
        new(
            Room: RoomDefinitions.BelowSpazer,
            Door: MaridiaDoorDefinitions.WestTunnelToBelowSpazer,
            CameraPosition: CameraAtScreen(new(1, 1)),
            Samus: AttractDemoSamusSetups.StandingLeft.OnFloor(470, floorRow: 26),
            Duration: 509,
            RoomSetup: null,
            Items: Mobility,
            Missiles: MissilesForTanks(6),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractIceBeam)),
        new(
            Room: RoomDefinitions.GreenBrinstarFirefleas,
            Door: BrinstarDoorDefinitions.GreenBrinstarMainShaftToGreenBrinstarFirefleas,
            CameraPosition: CameraAtScreen(new(2, 0)),
            Samus: AttractDemoSamusSetups.StandingLeft.OnFloor(713, floorRow: 10),
            Duration: 410,
            RoomSetup: null,
            Items: SamusEquipmentFlags.MorphBall,
            Missiles: MissilesForTanks(2),
            SuperMissiles: SuperMissilesForTanks(0),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractFirefleaRoom)),
        new(
            Room: RoomDefinitions.GreenHillZone,
            Door: BrinstarDoorDefinitions.BigPinkToGreenHillZone,
            CameraPosition: CameraAtScreen(new(5, 3)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(1378, floorRow: 60),
            Duration: 279,
            RoomSetup: null,
            Items: SamusEquipmentFlags.MorphBall,
            Missiles: MissilesForTanks(5),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractBrinstarDiagonalRoom)),
        new(
            Room: RoomDefinitions.LavaDive,
            Door: NorfairDoorDefinitions.KronicBoostToLavaDive,
            CameraPosition: CameraAtScreen(new(3, 0)),
            Samus: AttractDemoSamusSetups.StandingLeft.OnFloor(971, floorRow: 10),
            Duration: 970,
            RoomSetup: null,
            Items: SpaceJump,
            Missiles: MissilesForTanks(15),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(8),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractLowerNorfairEntrance)),
        new(
            Room: RoomDefinitions.Flyway,
            Door: CrateriaDoorDefinitions.ParlorToFlyway,
            CameraPosition: CameraAtScreen(new(0, 0)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(113, floorRow: 13),
            Duration: 213,
            RoomSetup: null,
            Items: ScrewAttack,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractScrewAttack)),
        new(
            Room: RoomDefinitions.Dachora,
            Door: BrinstarDoorDefinitions.BigPinkToDachora,
            CameraPosition: CameraAtScreen(new(4, 2)),
            Samus: AttractDemoSamusSetups.FallingLeft.At(new(1157, 640)),
            Duration: 791,
            RoomSetup: null,
            Items: Exploration,
            Missiles: MissilesForTanks(9),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractDachora)),
    ]);

    /// <summary>The set the title plays after <see cref="SecondSet"/>.</summary>
    public static AttractDemoSet ThirdSet { get; } = new(nameof(ThirdSet),
    [
        new(
            Room: RoomDefinitions.Basement,
            Door: WreckedShipDoorDefinitions.WreckedShipMainShaftToBasement,
            CameraPosition: CameraAtScreen(new(2, 0)),
            Samus: AttractDemoSamusSetups.StandingLeft.At(new(644, 96)),
            Duration: 751,
            RoomSetup: null,
            Items: Exploration,
            Missiles: MissilesForTanks(11),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractPrePhantoonHall)),
        new(
            Room: RoomDefinitions.LandingSite,
            Door: CrateriaDoorDefinitions.ParlorToLandingSite,
            CameraPosition: CameraAtScreen(new(3, 4)),
            Samus: AttractDemoSamusSetups.DiagonalShinespark.At(new(896, 1200)),
            Duration: 199,
            RoomSetup: null,
            Items: BombAbilities | SamusEquipmentFlags.SpringBall,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: AllBeams,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractDiagonalShinespark, Continuation: DemoInputRecordingId.AttractDiagonalShinesparkContinuation)),
        new(
            Room: RoomDefinitions.KraidEyeDoor,
            Door: BrinstarDoorDefinitions.MiniKraidToKraidEyeDoor,
            CameraPosition: CameraAtScreen(new(0, 1)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(82, floorRow: 26),
            Duration: 723,
            RoomSetup: null,
            Items: SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots,
            Missiles: MissilesForTanks(5),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Spazer | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractEyeDoor)),
        new(
            Room: RoomDefinitions.Caterpillar,
            Door: BrinstarDoorDefinitions.AlphaPowerBombToCaterpillar,
            CameraPosition: CameraAtScreen(new(0, 7)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(42, floorRow: 122),
            Duration: 356,
            RoomSetup: null,
            Items: Mobility,
            Missiles: MissilesForTanks(6),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractRedBrinstarElevator)),
        new(
            Room: RoomDefinitions.Kraid,
            Door: BrinstarDoorDefinitions.KraidEyeDoorToKraid,
            CameraPosition: CameraAtScreen(new(0, 1)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(63, floorRow: 26),
            Duration: 319,
            RoomSetup: AttractDemoRoomSetup.KraidTimer,
            Items: SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots,
            Missiles: MissilesForTanks(5),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractKraid)),
        new(
            Room: RoomDefinitions.Statues,
            Door: CrateriaDoorDefinitions.StatuesHallwayToStatues,
            CameraPosition: CameraAtScreen(new(0, 0)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(49, floorRow: 10),
            Duration: 407,
            RoomSetup: AttractDemoRoomSetup.DefeatedKraid,
            Items: Mobility,
            Missiles: MissilesForTanks(6),
            SuperMissiles: SuperMissilesForTanks(1),
            PowerBombs: PowerBombsForTanks(1),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractTourianEntrance)),
    ]);

    /// <summary>The advanced techniques the title plays after <see cref="ThirdSet"/> once a save records a completed game.</summary>
    private static AttractDemoSet CompletionSet { get; } = new(nameof(CompletionSet),
    [
        new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            Samus: AttractDemoSamusSetups.HorizontalShinespark.At(new(1712, 640)),
            Duration: 256,
            RoomSetup: null,
            Items: BombAbilities,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractGauntletEntrance)),
        new(
            Room: RoomDefinitions.MtEverest,
            Door: MaridiaDoorDefinitions.RedFishToMtEverest,
            CameraPosition: CameraAtScreen(new(2, 0)),
            Samus: AttractDemoSamusSetups.StandingLeft.OnFloor(640, floorRow: 12),
            Duration: 818,
            RoomSetup: null,
            Items: BombAbilities,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractAdvancedGrappleBeam)),
        new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            Samus: AttractDemoSamusSetups.StandingLeft.At(new(1696, 635)),
            Duration: 389,
            RoomSetup: null,
            Items: BombAbilities,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.None,
            EquippedBeams: SamusBeamFlags.None,
            Input: new(DemoInputRecordingId.AttractInfiniteBombJump)),
        new(
            Room: RoomDefinitions.GreenBrinstarMainShaft,
            Door: BrinstarDoorDefinitions.BrinstarPreMapToGreenBrinstarMainShaft,
            CameraPosition: CameraAtScreen(new(0, 4)),
            Samus: AttractDemoSamusSetups.StandingRight.OnFloor(55, floorRow: 74),
            Duration: 394,
            RoomSetup: null,
            Items: BombAbilities,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(3),
            PowerBombs: PowerBombsForTanks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Plasma | SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractSpecialBeamAttack)),
        new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            Samus: AttractDemoSamusSetups.LowHealthLeft.OnFloor(1668, floorRow: 42),
            Duration: 512,
            RoomSetup: null,
            Items: BombAbilities,
            Missiles: MissilesForTanks(17),
            SuperMissiles: SuperMissilesForTanks(4),
            PowerBombs: PowerBombsForTanks(4),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: SamusBeamFlags.Charge,
            EquippedBeams: SamusBeamFlags.Charge,
            Input: new(DemoInputRecordingId.AttractCrystalFlash)),
    ]);

    /// <summary>The sets the title cycles through, in order, while no save records a completed game.</summary>
    private static IReadOnlyList<AttractDemoSet> OrdinaryPlaylist { get; } = [FirstSet, SecondSet, ThirdSet];

    /// <summary>The sets the title cycles through, in order, once a save records a completed game.</summary>
    private static IReadOnlyList<AttractDemoSet> CompletedGamePlaylist { get; } = [FirstSet, SecondSet, ThirdSet, CompletionSet];

    /// <summary>The playlist the title cycles through: the completion set joins it only when the game was completed.</summary>
    public static IReadOnlyList<AttractDemoSet> Playlist(bool completionSetUnlocked) =>
        completionSetUnlocked ? CompletedGamePlaylist : OrdinaryPlaylist;

    /// <summary>Camera origins use whole 256-pixel room screens.</summary>
    private static Position16<RoomPixels> CameraAtScreen(Position16<RoomScreens> screen) => screen.ToRoomPixels();

    /// <summary>The missiles <paramref name="tanks"/> missile tanks give.</summary>
    private static int MissilesForTanks(int tanks) => tanks * CollectibleCapacityDefinitions.MissileTankMissiles;

    /// <summary>The super missiles <paramref name="tanks"/> super missile tanks give.</summary>
    private static int SuperMissilesForTanks(int tanks) => tanks * CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles;

    /// <summary>The power bombs <paramref name="tanks"/> power bomb tanks give.</summary>
    private static int PowerBombsForTanks(int tanks) => tanks * CollectibleCapacityDefinitions.PowerBombTankPowerBombs;

    /// <summary>Samus's energy capacity with <paramref name="tanks"/> energy tanks: her starting energy and each tank's.</summary>
    private static int HealthForEnergyTanks(int tanks) =>
        SamusEnergyDefinitions.StartingEnergy + tanks * SamusEnergyDefinitions.EnergyPerTank;
}
