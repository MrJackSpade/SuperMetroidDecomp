using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>Executable staging for the title demonstrations, joined across room, equipment and setup definitions.</summary>
/// <remarks>The exact playlist, initial staging and editorial cutoffs select this recorded demonstration performance. Physical alignment, capacity units, shared abilities and control calculate independently.</remarks>
public static class StockAttractDemoScenes
{
    /// <summary>Mutually exclusive demonstration identities.</summary>
    private enum Demonstration
    {
        /// <summary>The LandingSite demonstration.</summary>
        LandingSite,
        /// <summary>The MissileDoor demonstration.</summary>
        MissileDoor,
        /// <summary>The PreSporeSpawnHall demonstration.</summary>
        PreSporeSpawnHall,
        /// <summary>The SpeedBooster demonstration.</summary>
        SpeedBooster,
        /// <summary>The GrappleBeam demonstration.</summary>
        GrappleBeam,
        /// <summary>The PseudoScrewAttack demonstration.</summary>
        PseudoScrewAttack,
        /// <summary>The IceBeam demonstration.</summary>
        IceBeam,
        /// <summary>The FirefleaRoom demonstration.</summary>
        FirefleaRoom,
        /// <summary>The BrinstarDiagonalRoom demonstration.</summary>
        BrinstarDiagonalRoom,
        /// <summary>The LowerNorfairEntrance demonstration.</summary>
        LowerNorfairEntrance,
        /// <summary>The ScrewAttack demonstration.</summary>
        ScrewAttack,
        /// <summary>The Dachora demonstration.</summary>
        Dachora,
        /// <summary>The PrePhantoonHall demonstration.</summary>
        PrePhantoonHall,
        /// <summary>The DiagonalShinespark demonstration.</summary>
        DiagonalShinespark,
        /// <summary>The EyeDoor demonstration.</summary>
        EyeDoor,
        /// <summary>The RedBrinstarElevator demonstration.</summary>
        RedBrinstarElevator,
        /// <summary>The Kraid demonstration.</summary>
        Kraid,
        /// <summary>The TourianEntrance demonstration.</summary>
        TourianEntrance,
        /// <summary>The GauntletEntrance demonstration.</summary>
        GauntletEntrance,
        /// <summary>The AdvancedGrappleBeam demonstration.</summary>
        AdvancedGrappleBeam,
        /// <summary>The InfiniteBombJump demonstration.</summary>
        InfiniteBombJump,
        /// <summary>The SpecialBeamAttack demonstration.</summary>
        SpecialBeamAttack,
        /// <summary>The CrystalFlash demonstration.</summary>
        CrystalFlash,
    }

    /// <summary>The set the title plays first after power-on or a soft reset.</summary>
    public static AttractDemoSet FirstSet { get; } = Set(nameof(FirstSet),
        Demonstration.LandingSite, Demonstration.MissileDoor, Demonstration.PreSporeSpawnHall,
        Demonstration.SpeedBooster, Demonstration.GrappleBeam, Demonstration.PseudoScrewAttack);

    /// <summary>The set the title plays after <see cref="FirstSet"/>.</summary>
    public static AttractDemoSet SecondSet { get; } = Set(nameof(SecondSet),
        Demonstration.IceBeam, Demonstration.FirefleaRoom, Demonstration.BrinstarDiagonalRoom,
        Demonstration.LowerNorfairEntrance, Demonstration.ScrewAttack, Demonstration.Dachora);

    /// <summary>The set the title plays after <see cref="SecondSet"/>.</summary>
    public static AttractDemoSet ThirdSet { get; } = Set(nameof(ThirdSet),
        Demonstration.PrePhantoonHall, Demonstration.DiagonalShinespark, Demonstration.EyeDoor,
        Demonstration.RedBrinstarElevator, Demonstration.Kraid, Demonstration.TourianEntrance);

    /// <summary>The advanced techniques the title plays after <see cref="ThirdSet"/> once a save records a completed game.</summary>
    public static AttractDemoSet CompletionSet { get; } = Set(nameof(CompletionSet),
        Demonstration.GauntletEntrance, Demonstration.AdvancedGrappleBeam, Demonstration.InfiniteBombJump,
        Demonstration.SpecialBeamAttack, Demonstration.CrystalFlash);

    /// <summary>The sets the title cycles through, in order, while no save records a completed game.</summary>
    public static IReadOnlyList<AttractDemoSet> OrdinaryPlaylist { get; } = [FirstSet, SecondSet, ThirdSet];

    /// <summary>The sets the title cycles through, in order, once a save records a completed game.</summary>
    public static IReadOnlyList<AttractDemoSet> CompletedGamePlaylist { get; } = [FirstSet, SecondSet, ThirdSet, CompletionSet];

    /// <summary>The playlist the title cycles through: the completion set joins it only when the game was completed.</summary>
    public static IReadOnlyList<AttractDemoSet> Playlist(bool completionSetUnlocked) =>
        completionSetUnlocked ? CompletedGamePlaylist : OrdinaryPlaylist;

    /// <summary>The set named <paramref name="name"/>, staging <paramref name="demonstrations"/> in playing order.</summary>
    private static AttractDemoSet Set(string name, params Demonstration[] demonstrations) =>
        new(name, [.. demonstrations.Select(Stage)]);
    /// <summary>
    /// The chosen staging of each demonstration. Screen origins, upgrade capacities,
    /// and composable equipment flags calculate from their domain units; choosing each scene's
    /// starting configuration and editorial cutoff defines this recorded performance.
    /// </summary>
    private static AttractDemoScene Stage(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite => new(
            Room: RoomDefinitions.LandingSite,
            Door: CrateriaDoorDefinitions.Parlor1,
            CameraPosition: CameraAtScreen(new(4, 4)),
            SamusOffset: new(1, 64),
            Duration: 1235,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(0),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractLandingSite),
        Demonstration.MissileDoor => new(
            Room: RoomDefinitions.ConstructionZone,
            Door: BrinstarDoorDefinitions.MorphBall1,
            CameraPosition: CameraAtScreen(new(0, 0)),
            SamusOffset: new(-46, GroundedY(8, demonstration)),
            Duration: 337,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(1),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractMissileDoor),
        Demonstration.PreSporeSpawnHall => new(
            Room: RoomDefinitions.SporeSpawnKihunters,
            Door: BrinstarDoorDefinitions.BigPink0,
            CameraPosition: CameraAtScreen(new(1, 0)),
            SamusOffset: new(-32, GroundedY(13, demonstration)),
            Duration: 378,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(3),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractPreSporeSpawnHall),
        Demonstration.SpeedBooster => new(
            Room: RoomDefinitions.FrogSpeedway,
            Door: NorfairDoorDefinitions.UpperNorfairFarming0,
            CameraPosition: CameraAtScreen(new(7, 0)),
            SamusOffset: new(72, GroundedY(10, demonstration)),
            Duration: 420,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractSpeedBooster),
        Demonstration.GrappleBeam => new(
            Room: RoomDefinitions.SpikyAcidSnakesTunnel,
            Door: NorfairDoorDefinitions.UpperNorfairRefill1,
            CameraPosition: CameraAtScreen(new(0, 0)),
            SamusOffset: new(-62, GroundedY(10, demonstration)),
            Duration: 444,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractGrappleBeam),
        Demonstration.PseudoScrewAttack => new(
            Room: RoomDefinitions.BigPink,
            Door: BrinstarDoorDefinitions.GreenHillZone0,
            CameraPosition: CameraAtScreen(new(2, 6)),
            SamusOffset: new(39, GroundedY(10, demonstration)),
            Duration: 613,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(4),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractPseudoScrewAttack),
        Demonstration.IceBeam => new(
            Room: RoomDefinitions.BelowSpazer,
            Door: MaridiaDoorDefinitions.WestTunnel1,
            CameraPosition: CameraAtScreen(new(1, 1)),
            SamusOffset: new(86, GroundedY(10, demonstration)),
            Duration: 509,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractIceBeam),
        Demonstration.FirefleaRoom => new(
            Room: RoomDefinitions.GreenBrinstarFirefleas,
            Door: BrinstarDoorDefinitions.GreenBrinstarMainShaft3,
            CameraPosition: CameraAtScreen(new(2, 0)),
            SamusOffset: new(73, GroundedY(10, demonstration)),
            Duration: 410,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(2),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractFirefleaRoom),
        Demonstration.BrinstarDiagonalRoom => new(
            Room: RoomDefinitions.GreenHillZone,
            Door: BrinstarDoorDefinitions.BigPink3,
            CameraPosition: CameraAtScreen(new(5, 3)),
            SamusOffset: new(-30, GroundedY(12, demonstration)),
            Duration: 279,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractBrinstarDiagonalRoom),
        Demonstration.LowerNorfairEntrance => new(
            Room: RoomDefinitions.LavaDive,
            Door: NorfairDoorDefinitions.KronicBoost2,
            CameraPosition: CameraAtScreen(new(3, 0)),
            SamusOffset: new(75, GroundedY(10, demonstration)),
            Duration: 970,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(15),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(8),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractLowerNorfairEntrance),
        Demonstration.ScrewAttack => new(
            Room: RoomDefinitions.Flyway,
            Door: CrateriaDoorDefinitions.Parlor3,
            CameraPosition: CameraAtScreen(new(0, 0)),
            SamusOffset: new(-15, GroundedY(13, demonstration)),
            Duration: 213,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractScrewAttack),
        Demonstration.Dachora => new(
            Room: RoomDefinitions.Dachora,
            Door: BrinstarDoorDefinitions.BigPink1,
            CameraPosition: CameraAtScreen(new(4, 2)),
            SamusOffset: new(5, 128),
            Duration: 791,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(9),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractDachora),
        Demonstration.PrePhantoonHall => new(
            Room: RoomDefinitions.Basement,
            Door: WreckedShipDoorDefinitions.WreckedShipMainShaft3,
            CameraPosition: CameraAtScreen(new(2, 0)),
            SamusOffset: new(4, 96),
            Duration: 751,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(11),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractPrePhantoonHall),
        Demonstration.DiagonalShinespark => new(
            Room: RoomDefinitions.LandingSite,
            Door: CrateriaDoorDefinitions.Parlor1,
            CameraPosition: CameraAtScreen(new(3, 4)),
            SamusOffset: new(0, 176),
            Duration: 199,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractDiagonalShinespark),
        Demonstration.EyeDoor => new(
            Room: RoomDefinitions.KraidEyeDoor,
            Door: BrinstarDoorDefinitions.MiniKraid1,
            CameraPosition: CameraAtScreen(new(0, 1)),
            SamusOffset: new(-46, GroundedY(10, demonstration)),
            Duration: 723,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractEyeDoor),
        Demonstration.RedBrinstarElevator => new(
            Room: RoomDefinitions.Caterpillar,
            Door: BrinstarDoorDefinitions.AlphaPowerBomb0,
            CameraPosition: CameraAtScreen(new(0, 7)),
            SamusOffset: new(-86, GroundedY(10, demonstration)),
            Duration: 356,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractRedBrinstarElevator),
        Demonstration.Kraid => new(
            Room: RoomDefinitions.Kraid,
            Door: BrinstarDoorDefinitions.KraidEyeDoor1,
            CameraPosition: CameraAtScreen(new(0, 1)),
            SamusOffset: new(-65, GroundedY(10, demonstration)),
            Duration: 319,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractKraid),
        Demonstration.TourianEntrance => new(
            Room: RoomDefinitions.Statues,
            Door: CrateriaDoorDefinitions.StatuesHallway1,
            CameraPosition: CameraAtScreen(new(0, 0)),
            SamusOffset: new(-79, GroundedY(10, demonstration)),
            Duration: 407,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractTourianEntrance),
        Demonstration.GauntletEntrance => new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            SamusOffset: new(48, 128),
            Duration: 256,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractGauntletEntrance),
        Demonstration.AdvancedGrappleBeam => new(
            Room: RoomDefinitions.MtEverest,
            Door: MaridiaDoorDefinitions.RedFish0,
            CameraPosition: CameraAtScreen(new(2, 0)),
            SamusOffset: new(0, GroundedY(12, demonstration)),
            Duration: 818,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractAdvancedGrappleBeam),
        Demonstration.InfiniteBombJump => new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            SamusOffset: new(32, 123),
            Duration: 389,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractInfiniteBombJump),
        Demonstration.SpecialBeamAttack => new(
            Room: RoomDefinitions.GreenBrinstarMainShaft,
            Door: BrinstarDoorDefinitions.BrinstarPreMap1,
            CameraPosition: CameraAtScreen(new(0, 4)),
            SamusOffset: new(-73, GroundedY(10, demonstration)),
            Duration: 394,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractSpecialBeamAttack),
        Demonstration.CrystalFlash => new(
            Room: RoomDefinitions.LandingSite,
            Door: DoorDefinitions.LandingSiteDemoEntry,
            CameraPosition: CameraAtScreen(new(6, 2)),
            SamusOffset: new(4, GroundedY(10, demonstration)),
            Duration: 512,
            RoomSetup: RoomSetupFor(demonstration),
            SamusSetup: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(4),
            PowerBombs: AmmoForPacks(4),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            Input: AttractDemoInputDefinitions.AttractCrystalFlash),
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>Seventeen staged actors stand on identified flat floor cells. Their selected room-screen floor rows convert to pixels, then subtract the actual immutable pose radius. This does not use editable room artwork or infer the six other placements.</summary>
    private static int GroundedY(int floorRow, Demonstration demonstration)
    {
        SamusPoseId pose = SamusSetupFor(demonstration) switch
        {
            AttractDemoSamusSetup.StandingRight => SamusPoseId.FacingRightNormalPose,
            AttractDemoSamusSetup.StandingLeft or AttractDemoSamusSetup.LowHealthLeft => SamusPoseId.FacingLeftNormalPose,
            AttractDemoSamusSetup.MorphLeft => SamusPoseId.MorphBallMovingLeftPose,
            AttractDemoSamusSetup.LandingSite or AttractDemoSamusSetup.FallingLeft or
                AttractDemoSamusSetup.DiagonalShinespark or AttractDemoSamusSetup.HorizontalShinespark =>
                throw new InvalidOperationException("This demonstration does not use a grounded staging pose."),
            _ => throw new InvalidOperationException($"Undefined AttractDemoSamusSetup {SamusSetupFor(demonstration)}."),
        };
        return CoordinateAxis.AsSigned(floorRow * 16 - SamusPoseCollisionRadii.ReadVerticalRadius(pose));
    }

    /// <summary>Landing Site entries select their sky, the charge-beam-room demonstration selects its scroll correction, and the two boss scenarios select their explicit room setup. All other staged rooms use the empty setup.</summary>
    private static AttractDemoRoomSetup RoomSetupFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite or Demonstration.DiagonalShinespark or Demonstration.GauntletEntrance or
        Demonstration.InfiniteBombJump or Demonstration.CrystalFlash => AttractDemoRoomSetup.LandingSiteSky,
        Demonstration.PseudoScrewAttack => AttractDemoRoomSetup.ChargeBeamScroll,
        Demonstration.Kraid => AttractDemoRoomSetup.KraidTimer,
        Demonstration.TourianEntrance => AttractDemoRoomSetup.DefeatedKraid,
        Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.SpeedBooster or
        Demonstration.GrappleBeam or Demonstration.IceBeam or Demonstration.FirefleaRoom or
        Demonstration.BrinstarDiagonalRoom or Demonstration.LowerNorfairEntrance or Demonstration.ScrewAttack or
        Demonstration.Dachora or Demonstration.PrePhantoonHall or Demonstration.EyeDoor or
        Demonstration.RedBrinstarElevator or Demonstration.AdvancedGrappleBeam or
        Demonstration.SpecialBeamAttack => AttractDemoRoomSetup.NoOp,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration), demonstration, "Undefined attract demonstration."),
    };

    /// <summary>Selected actor setup roles for the recorded demonstrations. Shared standing directions are set up once; the front-facing, morph, fall, spark and low-health performances keep their distinct setups.</summary>
    private static AttractDemoSamusSetup SamusSetupFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite => AttractDemoSamusSetup.LandingSite,
        Demonstration.PseudoScrewAttack => AttractDemoSamusSetup.MorphLeft,
        Demonstration.Dachora => AttractDemoSamusSetup.FallingLeft,
        Demonstration.DiagonalShinespark => AttractDemoSamusSetup.DiagonalShinespark,
        Demonstration.GauntletEntrance => AttractDemoSamusSetup.HorizontalShinespark,
        Demonstration.CrystalFlash => AttractDemoSamusSetup.LowHealthLeft,
        Demonstration.SpeedBooster or Demonstration.IceBeam or Demonstration.FirefleaRoom or
        Demonstration.LowerNorfairEntrance or Demonstration.PrePhantoonHall or
        Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump => AttractDemoSamusSetup.StandingLeft,
        Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.GrappleBeam or
        Demonstration.BrinstarDiagonalRoom or Demonstration.ScrewAttack or Demonstration.EyeDoor or
        Demonstration.RedBrinstarElevator or Demonstration.Kraid or Demonstration.TourianEntrance or
        Demonstration.SpecialBeamAttack => AttractDemoSamusSetup.StandingRight,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>Chosen loadouts share actual item abilities, rather than repeating full equipment sets. Stage membership and additional abilities remain selected performance inputs; the unassigned shinespark flag is preserved without inventing an ability.</summary>
    private static SamusEquipmentFlags ItemsFor(Demonstration demonstration)
    {
        const SamusEquipmentFlags mobility = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpeedBooster;
        const SamusEquipmentFlags exploration = mobility | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope;
        const SamusEquipmentFlags spaceJump = exploration | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpaceJump;
        const SamusEquipmentFlags screwAttack = spaceJump | SamusEquipmentFlags.ScrewAttack;
        const SamusEquipmentFlags bombAbilities = screwAttack | SamusEquipmentFlags.Bombs;
        return demonstration switch
        {
            Demonstration.LandingSite => SamusEquipmentFlags.None,
            Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.PseudoScrewAttack or
            Demonstration.FirefleaRoom or Demonstration.BrinstarDiagonalRoom => SamusEquipmentFlags.MorphBall,
            Demonstration.SpeedBooster or Demonstration.IceBeam or Demonstration.RedBrinstarElevator or
            Demonstration.TourianEntrance => mobility,
            Demonstration.GrappleBeam => mobility | SamusEquipmentFlags.GrappleBeam,
            Demonstration.Dachora or Demonstration.PrePhantoonHall => exploration,
            Demonstration.LowerNorfairEntrance => spaceJump,
            Demonstration.ScrewAttack => screwAttack,
            Demonstration.DiagonalShinespark => bombAbilities | SamusEquipmentFlags.SpringBall,
            Demonstration.EyeDoor or Demonstration.Kraid => (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots),
            Demonstration.GauntletEntrance or Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump or
            Demonstration.SpecialBeamAttack or Demonstration.CrystalFlash => bombAbilities,
            _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
        };
    }

    /// <summary>
    /// Equipped-beam selections for each demonstration. These choose composable
    /// beam abilities; the selected per-scene loadouts belong to this recorded demonstration.
    /// </summary>
    private static SuperMetroid.Core.Progression.SamusBeamFlags EquippedBeamsFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.PreSporeSpawnHall or Demonstration.PseudoScrewAttack or Demonstration.BrinstarDiagonalRoom or Demonstration.LowerNorfairEntrance or Demonstration.Dachora or Demonstration.PrePhantoonHall or Demonstration.DiagonalShinespark or Demonstration.GauntletEntrance or Demonstration.CrystalFlash => SamusBeamFlags.Charge,
        Demonstration.GrappleBeam or Demonstration.IceBeam => (SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge),
        Demonstration.TourianEntrance or Demonstration.SpecialBeamAttack => (SamusBeamFlags.Plasma | SamusBeamFlags.Charge),
        Demonstration.SpeedBooster or Demonstration.EyeDoor => (SamusBeamFlags.Spazer | SamusBeamFlags.Charge),
        Demonstration.LandingSite or Demonstration.MissileDoor or Demonstration.FirefleaRoom or Demonstration.ScrewAttack or Demonstration.RedBrinstarElevator or Demonstration.Kraid or Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump => SamusBeamFlags.None,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>
    /// Collected beams equal equipped beams in 22 scenes. The diagonal-shinespark
    /// scene collects all beam types but equips only Charge.
    /// </summary>
    private static SuperMetroid.Core.Progression.SamusBeamFlags CollectedBeamsFor(Demonstration demonstration) =>
        demonstration == Demonstration.DiagonalShinespark
            ? (SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)
            : EquippedBeamsFor(demonstration);

    /// <summary>Camera origins use whole 256-pixel room screens.</summary>
    private static Position16<RoomPixels> CameraAtScreen(Position16<RoomScreens> screen) => screen.ToRoomPixels();

    /// <summary>Ammo counts are a number of five-round upgrades.</summary>
    private static int AmmoForPacks(int packs) => packs * 5;

    /// <summary>Energy capacities consist of 99 base energy plus 100 per tank.</summary>
    private static int HealthForEnergyTanks(int tanks) => 99 + tanks * 100;
}
