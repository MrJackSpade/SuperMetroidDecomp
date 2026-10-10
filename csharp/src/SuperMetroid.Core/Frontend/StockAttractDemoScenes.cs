using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Executable staging for the native title demonstrations, joined across room, equipment and setup definitions.</summary>
/// <remarks>The exact playlist, initial staging and editorial cutoffs select this recorded demonstration performance. Physical alignment, capacity units, shared abilities and control dispatch calculate independently.</remarks>
public static class StockAttractDemoScenes
{
    /// <summary>Mutually exclusive demonstration identities, each naming its native bank-91 input object.</summary>
    private enum Demonstration : ushort
    {
        /// <summary><c>$91:9E52</c> input object; <c>$82:8774</c> room staging for LandingSite.</summary>
        LandingSite = 0x9e52,
        /// <summary><c>$91:9E88</c> input object; <c>$82:8786</c> room staging for MissileDoor.</summary>
        MissileDoor = 0x9e88,
        /// <summary><c>$91:9EAC</c> input object; <c>$82:8798</c> room staging for PreSporeSpawnHall.</summary>
        PreSporeSpawnHall = 0x9eac,
        /// <summary><c>$91:9E5E</c> input object; <c>$82:87AA</c> room staging for SpeedBooster.</summary>
        SpeedBooster = 0x9e5e,
        /// <summary><c>$91:9EB2</c> input object; <c>$82:87BC</c> room staging for GrappleBeam.</summary>
        GrappleBeam = 0x9eb2,
        /// <summary><c>$91:9E58</c> input object; <c>$82:87CE</c> room staging for PseudoScrewAttack.</summary>
        PseudoScrewAttack = 0x9e58,
        /// <summary><c>$91:9EB8</c> input object; <c>$82:87E2</c> room staging for IceBeam.</summary>
        IceBeam = 0x9eb8,
        /// <summary><c>$91:9E94</c> input object; <c>$82:87F4</c> room staging for FirefleaRoom.</summary>
        FirefleaRoom = 0x9e94,
        /// <summary><c>$91:9EA0</c> input object; <c>$82:8806</c> room staging for BrinstarDiagonalRoom.</summary>
        BrinstarDiagonalRoom = 0x9ea0,
        /// <summary><c>$91:9E76</c> input object; <c>$82:8818</c> room staging for LowerNorfairEntrance.</summary>
        LowerNorfairEntrance = 0x9e76,
        /// <summary><c>$91:9E9A</c> input object; <c>$82:882A</c> room staging for ScrewAttack.</summary>
        ScrewAttack = 0x9e9a,
        /// <summary><c>$91:9E64</c> input object; <c>$82:883C</c> room staging for Dachora.</summary>
        Dachora = 0x9e64,
        /// <summary><c>$91:9E70</c> input object; <c>$82:8850</c> room staging for PrePhantoonHall.</summary>
        PrePhantoonHall = 0x9e70,
        /// <summary><c>$91:9E82</c> input object; <c>$82:8862</c> room staging for DiagonalShinespark.</summary>
        DiagonalShinespark = 0x9e82,
        /// <summary><c>$91:9E7C</c> input object; <c>$82:8874</c> room staging for EyeDoor.</summary>
        EyeDoor = 0x9e7c,
        /// <summary><c>$91:9E6A</c> input object; <c>$82:8886</c> room staging for RedBrinstarElevator.</summary>
        RedBrinstarElevator = 0x9e6a,
        /// <summary><c>$91:9E8E</c> input object; <c>$82:8898</c> room staging for Kraid.</summary>
        Kraid = 0x9e8e,
        /// <summary><c>$91:9ED6</c> input object; <c>$82:88AA</c> room staging for TourianEntrance.</summary>
        TourianEntrance = 0x9ed6,
        /// <summary><c>$91:9EBE</c> input object; <c>$82:88BE</c> room staging for GauntletEntrance.</summary>
        GauntletEntrance = 0x9ebe,
        /// <summary><c>$91:9EC4</c> input object; <c>$82:88D0</c> room staging for AdvancedGrappleBeam.</summary>
        AdvancedGrappleBeam = 0x9ec4,
        /// <summary><c>$91:9ECA</c> input object; <c>$82:88E2</c> room staging for InfiniteBombJump.</summary>
        InfiniteBombJump = 0x9eca,
        /// <summary><c>$91:9ED0</c> input object; <c>$82:88F4</c> room staging for SpecialBeamAttack.</summary>
        SpecialBeamAttack = 0x9ed0,
        /// <summary><c>$91:9EDC</c> input object; <c>$82:8906</c> room staging for CrystalFlash.</summary>
        CrystalFlash = 0x9edc,
    }

    /// <summary>
    /// $91:895D sets equipment bit $0010 (unassigned in the named item domain) alongside the named item abilities in $F33F.
    /// Preserve the exact staged word without assigning this bit an invented ability;
    /// this chosen copied metadata is part of the exact demonstration staging, not an additional named ability.
    /// </summary>
    private const ushort ChosenShinesparkUnassignedItemBit = 0x0010;
    /// <summary>Returns a staged demonstration or its exact native end-of-set sentinel.</summary>
    public static AttractDemoScene? Get(int set, int scene)
    {
        if ((uint)set >= AttractDemoRomData.SetCount) throw new ArgumentOutOfRangeException(nameof(set));
        int sceneCount = set == 3 ? 5 : 6;
        if ((uint)scene > sceneCount) throw new ArgumentOutOfRangeException(nameof(scene));
        if (scene == sceneCount) return null;
        Demonstration demonstration = (set, scene) switch
        {
            (0, 0) => Demonstration.LandingSite,
            (0, 1) => Demonstration.MissileDoor,
            (0, 2) => Demonstration.PreSporeSpawnHall,
            (0, 3) => Demonstration.SpeedBooster,
            (0, 4) => Demonstration.GrappleBeam,
            (0, 5) => Demonstration.PseudoScrewAttack,
            (1, 0) => Demonstration.IceBeam,
            (1, 1) => Demonstration.FirefleaRoom,
            (1, 2) => Demonstration.BrinstarDiagonalRoom,
            (1, 3) => Demonstration.LowerNorfairEntrance,
            (1, 4) => Demonstration.ScrewAttack,
            (1, 5) => Demonstration.Dachora,
            (2, 0) => Demonstration.PrePhantoonHall,
            (2, 1) => Demonstration.DiagonalShinespark,
            (2, 2) => Demonstration.EyeDoor,
            (2, 3) => Demonstration.RedBrinstarElevator,
            (2, 4) => Demonstration.Kraid,
            (2, 5) => Demonstration.TourianEntrance,
            (3, 0) => Demonstration.GauntletEntrance,
            (3, 1) => Demonstration.AdvancedGrappleBeam,
            (3, 2) => Demonstration.InfiniteBombJump,
            (3, 3) => Demonstration.SpecialBeamAttack,
            (3, 4) => Demonstration.CrystalFlash,
            _ => throw new InvalidOperationException("Validated attract playlist position has no demonstration."),
        };
        return Stage(demonstration);
    }

    /// <summary>
    /// The chosen staging at $82:8774-8918 and $91:888D-8A32. Screen origins, upgrade capacities,
    /// and composable equipment words calculate from their domain units; choosing each scene's
    /// starting configuration and editorial cutoff defines this recorded performance.
    /// </summary>
    private static AttractDemoScene Stage(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite => new(
            RoomPointer: 0x91f8,
            DoorPointer: 0x896a,
            CameraX: CameraForScreens(4),
            CameraY: CameraForScreens(4),
            SamusYFromTop: 64,
            SamusXFromCenter: 1,
            Duration: 1235,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(0),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.MissileDoor => new(
            RoomPointer: 0x9f11,
            DoorPointer: 0x8eaa,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(8, demonstration),
            SamusXFromCenter: -46,
            Duration: 337,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(1),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(0),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.PreSporeSpawnHall => new(
            RoomPointer: 0x9d9c,
            DoorPointer: 0x8dc6,
            CameraX: CameraForScreens(1),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(13, demonstration),
            SamusXFromCenter: -32,
            Duration: 378,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(3),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.SpeedBooster => new(
            RoomPointer: 0xb106,
            DoorPointer: 0x970e,
            CameraX: CameraForScreens(7),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 72,
            Duration: 420,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.GrappleBeam => new(
            RoomPointer: 0xaffb,
            DoorPointer: 0x9792,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -62,
            Duration: 444,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.PseudoScrewAttack => new(
            RoomPointer: 0x9d19,
            DoorPointer: 0x8e7a,
            CameraX: CameraForScreens(2),
            CameraY: CameraForScreens(6),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 39,
            Duration: 613,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(4),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.IceBeam => new(
            RoomPointer: 0xa408,
            DoorPointer: 0xa36c,
            CameraX: CameraForScreens(1),
            CameraY: CameraForScreens(1),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 86,
            Duration: 509,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.FirefleaRoom => new(
            RoomPointer: 0x9c5e,
            DoorPointer: 0x8cca,
            CameraX: CameraForScreens(2),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 73,
            Duration: 410,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(2),
            SuperMissiles: AmmoForPacks(0),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.BrinstarDiagonalRoom => new(
            RoomPointer: 0x9e52,
            DoorPointer: 0x8dea,
            CameraX: CameraForScreens(5),
            CameraY: CameraForScreens(3),
            SamusYFromTop: GroundedY(12, demonstration),
            SamusXFromCenter: -30,
            Duration: 279,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(1),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.LowerNorfairEntrance => new(
            RoomPointer: 0xaf14,
            DoorPointer: 0x967e,
            CameraX: CameraForScreens(3),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 75,
            Duration: 970,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(15),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(8),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.ScrewAttack => new(
            RoomPointer: 0x9879,
            DoorPointer: 0x8982,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(13, demonstration),
            SamusXFromCenter: -15,
            Duration: 213,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.Dachora => new(
            RoomPointer: 0x9cb3,
            DoorPointer: 0x8dd2,
            CameraX: CameraForScreens(4),
            CameraY: CameraForScreens(2),
            SamusYFromTop: 128,
            SamusXFromCenter: 5,
            Duration: 791,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(9),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.PrePhantoonHall => new(
            RoomPointer: 0xcc6f,
            DoorPointer: 0xa21c,
            CameraX: CameraForScreens(2),
            CameraY: CameraForScreens(0),
            SamusYFromTop: 96,
            SamusXFromCenter: 4,
            Duration: 751,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(11),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(3),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.DiagonalShinespark => new(
            RoomPointer: 0x91f8,
            DoorPointer: 0x896a,
            CameraX: CameraForScreens(3),
            CameraY: CameraForScreens(4),
            SamusYFromTop: 176,
            SamusXFromCenter: 0,
            Duration: 199,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.EyeDoor => new(
            RoomPointer: 0xa56b,
            DoorPointer: 0x919e,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(1),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -46,
            Duration: 723,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.RedBrinstarElevator => new(
            RoomPointer: 0xa322,
            DoorPointer: 0x90ea,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(7),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -86,
            Duration: 356,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.Kraid => new(
            RoomPointer: 0xa59f,
            DoorPointer: 0x91b6,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(1),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -65,
            Duration: 319,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(5),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(0),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.TourianEntrance => new(
            RoomPointer: 0xa66a,
            DoorPointer: 0x91f2,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -79,
            Duration: 407,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(6),
            SuperMissiles: AmmoForPacks(1),
            PowerBombs: AmmoForPacks(1),
            Health: HealthForEnergyTanks(2),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.GauntletEntrance => new(
            RoomPointer: 0x91f8,
            DoorPointer: 0x890a,
            CameraX: CameraForScreens(6),
            CameraY: CameraForScreens(2),
            SamusYFromTop: 128,
            SamusXFromCenter: 48,
            Duration: 256,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.AdvancedGrappleBeam => new(
            RoomPointer: 0xd0b9,
            DoorPointer: 0xa474,
            CameraX: CameraForScreens(2),
            CameraY: CameraForScreens(0),
            SamusYFromTop: GroundedY(12, demonstration),
            SamusXFromCenter: 0,
            Duration: 818,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.InfiniteBombJump => new(
            RoomPointer: 0x91f8,
            DoorPointer: 0x890a,
            CameraX: CameraForScreens(6),
            CameraY: CameraForScreens(2),
            SamusYFromTop: 123,
            SamusXFromCenter: 32,
            Duration: 389,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.SpecialBeamAttack => new(
            RoomPointer: 0x9ad9,
            DoorPointer: 0x8d42,
            CameraX: CameraForScreens(0),
            CameraY: CameraForScreens(4),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: -73,
            Duration: 394,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(3),
            PowerBombs: AmmoForPacks(2),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        Demonstration.CrystalFlash => new(
            RoomPointer: 0x91f8,
            DoorPointer: 0x890a,
            CameraX: CameraForScreens(6),
            CameraY: CameraForScreens(2),
            SamusYFromTop: GroundedY(10, demonstration),
            SamusXFromCenter: 4,
            Duration: 512,
            RoomSetupPointer: RoomSetupFor(demonstration),
            SamusSetupPointer: SamusSetupFor(demonstration),
            Items: ItemsFor(demonstration),
            Missiles: AmmoForPacks(17),
            SuperMissiles: AmmoForPacks(4),
            PowerBombs: AmmoForPacks(4),
            Health: HealthForEnergyTanks(9),
            CollectedBeams: CollectedBeamsFor(demonstration),
            EquippedBeams: EquippedBeamsFor(demonstration),
            InputObject: (ushort)demonstration),
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>$82:8774-8918: seventeen staged actors stand on identified flat native floor cells. Their selected room-screen floor rows convert to pixels, then subtract the actual immutable pose radius. This does not use editable room artwork or infer the six other placements.</summary>
    private static ushort GroundedY(int floorRow, Demonstration demonstration)
    {
        byte pose = SamusSetupFor(demonstration) switch
        {
            AttractDemoRomData.SamusSetup.StandingRight => SamusPoseIds.FacingRightNormalPose,
            AttractDemoRomData.SamusSetup.StandingLeft or AttractDemoRomData.SamusSetup.LowHealthLeft => SamusPoseIds.FacingLeftNormalPose,
            AttractDemoRomData.SamusSetup.MorphLeft => SamusPoseIds.MorphBallMovingLeftPose,
            _ => throw new InvalidOperationException("This demonstration does not use a grounded staging pose."),
        };
        return (ushort)(floorRow * 16 - SamusPoseCollisionDefinitions.ReadVerticalRadius(pose));
    }

    /// <summary>$82:8774-8918: Landing Site entries select their sky, the charge-beam-room demonstration selects its scroll correction, and the two boss scenarios select their explicit room setup. All other staged rooms use the native no-op callback.</summary>
    private static ushort RoomSetupFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite or Demonstration.DiagonalShinespark or Demonstration.GauntletEntrance or
        Demonstration.InfiniteBombJump or Demonstration.CrystalFlash => AttractDemoRomData.RoomSetup.LandingSiteSky,
        Demonstration.PseudoScrewAttack => AttractDemoRomData.RoomSetup.ChargeBeamScroll,
        Demonstration.Kraid => AttractDemoRomData.RoomSetup.KraidTimer,
        Demonstration.TourianEntrance => AttractDemoRomData.RoomSetup.DefeatedKraid,
        Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.SpeedBooster or
        Demonstration.GrappleBeam or Demonstration.IceBeam or Demonstration.FirefleaRoom or
        Demonstration.BrinstarDiagonalRoom or Demonstration.LowerNorfairEntrance or Demonstration.ScrewAttack or
        Demonstration.Dachora or Demonstration.PrePhantoonHall or Demonstration.EyeDoor or
        Demonstration.RedBrinstarElevator or Demonstration.AdvancedGrappleBeam or
        Demonstration.SpecialBeamAttack => AttractDemoRomData.RoomSetup.NoOp,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration), demonstration, "Undefined attract demonstration."),
    };

    /// <summary>$91:89FD-8A32: selected actor setup roles for the recorded demonstrations. Shared standing directions dispatch once; the front-facing, morph, fall, spark and low-health performances keep their distinct native callbacks.</summary>
    private static ushort SamusSetupFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.LandingSite => AttractDemoRomData.SamusSetup.LandingSite,
        Demonstration.PseudoScrewAttack => AttractDemoRomData.SamusSetup.MorphLeft,
        Demonstration.Dachora => AttractDemoRomData.SamusSetup.FallingLeft,
        Demonstration.DiagonalShinespark => AttractDemoRomData.SamusSetup.DiagonalShinespark,
        Demonstration.GauntletEntrance => AttractDemoRomData.SamusSetup.HorizontalShinespark,
        Demonstration.CrystalFlash => AttractDemoRomData.SamusSetup.LowHealthLeft,
        Demonstration.SpeedBooster or Demonstration.IceBeam or Demonstration.FirefleaRoom or
        Demonstration.LowerNorfairEntrance or Demonstration.PrePhantoonHall or
        Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump => AttractDemoRomData.SamusSetup.StandingLeft,
        Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.GrappleBeam or
        Demonstration.BrinstarDiagonalRoom or Demonstration.ScrewAttack or Demonstration.EyeDoor or
        Demonstration.RedBrinstarElevator or Demonstration.Kraid or Demonstration.TourianEntrance or
        Demonstration.SpecialBeamAttack => AttractDemoRomData.SamusSetup.StandingRight,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>$91:888D-89FC: chosen loadouts share actual item abilities, rather than repeating full equipment words. Stage membership and additional abilities remain selected performance inputs; the unassigned shinespark bit is preserved without inventing an ability.</summary>
    private static ushort ItemsFor(Demonstration demonstration)
    {
        const SamusEquipmentFlags mobility = SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpeedBooster;
        const SamusEquipmentFlags exploration = mobility | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope;
        const SamusEquipmentFlags spaceJump = exploration | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpaceJump;
        const SamusEquipmentFlags screwAttack = spaceJump | SamusEquipmentFlags.ScrewAttack;
        const SamusEquipmentFlags bombAbilities = screwAttack | SamusEquipmentFlags.Bombs;
        return demonstration switch
        {
            Demonstration.LandingSite => (ushort)SamusEquipmentFlags.None,
            Demonstration.MissileDoor or Demonstration.PreSporeSpawnHall or Demonstration.PseudoScrewAttack or
            Demonstration.FirefleaRoom or Demonstration.BrinstarDiagonalRoom => (ushort)SamusEquipmentFlags.MorphBall,
            Demonstration.SpeedBooster or Demonstration.IceBeam or Demonstration.RedBrinstarElevator or
            Demonstration.TourianEntrance => (ushort)mobility,
            Demonstration.GrappleBeam => (ushort)(mobility | SamusEquipmentFlags.GrappleBeam),
            Demonstration.Dachora or Demonstration.PrePhantoonHall => (ushort)exploration,
            Demonstration.LowerNorfairEntrance => (ushort)spaceJump,
            Demonstration.ScrewAttack => (ushort)screwAttack,
            Demonstration.DiagonalShinespark => (ushort)((ushort)(bombAbilities | SamusEquipmentFlags.SpringBall) |
                ChosenShinesparkUnassignedItemBit),
            Demonstration.EyeDoor or Demonstration.Kraid => (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots),
            Demonstration.GauntletEntrance or Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump or
            Demonstration.SpecialBeamAttack or Demonstration.CrystalFlash => (ushort)bombAbilities,
            _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
        };
    }

    /// <summary>
    /// $91:8899-89F9 equipped-beam selections for each demonstration. These choose composable
    /// native beam abilities; the selected per-scene loadouts belong to this recorded demonstration.
    /// </summary>
    private static ushort EquippedBeamsFor(Demonstration demonstration) => demonstration switch
    {
        Demonstration.PreSporeSpawnHall or Demonstration.PseudoScrewAttack or Demonstration.BrinstarDiagonalRoom or Demonstration.LowerNorfairEntrance or Demonstration.Dachora or Demonstration.PrePhantoonHall or Demonstration.DiagonalShinespark or Demonstration.GauntletEntrance or Demonstration.CrystalFlash => (ushort)(SamusBeamFlags.Charge),
        Demonstration.GrappleBeam or Demonstration.IceBeam => (ushort)(SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Charge),
        Demonstration.TourianEntrance or Demonstration.SpecialBeamAttack => (ushort)(SamusBeamFlags.Plasma | SamusBeamFlags.Charge),
        Demonstration.SpeedBooster or Demonstration.EyeDoor => (ushort)(SamusBeamFlags.Spazer | SamusBeamFlags.Charge),
        Demonstration.LandingSite or Demonstration.MissileDoor or Demonstration.FirefleaRoom or Demonstration.ScrewAttack or Demonstration.RedBrinstarElevator or Demonstration.Kraid or Demonstration.AdvancedGrappleBeam or Demonstration.InfiniteBombJump => (ushort)SamusBeamFlags.None,
        _ => throw new ArgumentOutOfRangeException(nameof(demonstration)),
    };

    /// <summary>
    /// $91:888D-89FC: collected beams equal equipped beams in22 scenes. The diagonal-shinespark
    /// scene at $91:895D collects all beam types but equips only Charge.
    /// </summary>
    private static ushort CollectedBeamsFor(Demonstration demonstration) =>
        demonstration == Demonstration.DiagonalShinespark
            ? (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)
            : EquippedBeamsFor(demonstration);

    /// <summary>$82:86A9-86B8 camera origins use whole256-pixel room screens.</summary>
    private static ushort CameraForScreens(int screens) => (ushort)(screens * 256);

    /// <summary>$91:888D-89FC ammo fields express the number of native five-round upgrades.</summary>
    private static ushort AmmoForPacks(int packs) => (ushort)(packs * 5);

    /// <summary>$91:888D-89FC energy capacities consist of99 base energy plus100 per tank.</summary>
    private static ushort HealthForEnergyTanks(int tanks) => (ushort)(99 + tanks * 100);
}
