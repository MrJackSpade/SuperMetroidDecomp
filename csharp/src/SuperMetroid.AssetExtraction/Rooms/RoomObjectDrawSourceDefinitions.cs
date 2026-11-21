using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where the cartridge stores each PLM draw (#1402): the import pipeline's private knowledge, used to read and verify
/// a draw's level words when its artwork is extracted. Core identifies draws only semantically.
/// </summary>
[EncodingOwner("bank-$84 addresses of the PLM draws")]
internal static partial class RoomObjectDrawSourceDefinitions
{
    /// <summary>Bank $84, where every PLM draw is stored.</summary>
    private const byte Bank = 0x84;

    /// <summary>The address of each <see cref="BombBlockRestoreDraw"/>.</summary>
    internal static IReadOnlyDictionary<BombBlockRestoreDraw, SnesAddress> BombBlockRestore { get; } = new Dictionary<BombBlockRestoreDraw, SnesAddress>
    {
        [new BombBlockRestoreDraw(BlockArrangement.Horizontal)] = new(Bank, 0xa4c7),
        [new BombBlockRestoreDraw(BlockArrangement.Vertical)] = new(Bank, 0xa4cf),
        [new BombBlockRestoreDraw(BlockArrangement.Square)] = new(Bank, 0xa4d7),
    };

    /// <summary>The address of each <see cref="BombTorizoHandDraw"/>.</summary>
    internal static IReadOnlyDictionary<BombTorizoHandDraw, SnesAddress> BombTorizoHand { get; } = new Dictionary<BombTorizoHandDraw, SnesAddress>
    {
        [BombTorizoHandDraw.Intact] = new(Bank, 0x9877),
        [BombTorizoHandDraw.Cleared] = new(Bank, 0x989d),
    };

    /// <summary>The address of each <see cref="BotwoonWallDraw"/>.</summary>
    internal static IReadOnlyDictionary<BotwoonWallDraw, SnesAddress> BotwoonWall { get; } = new Dictionary<BotwoonWallDraw, SnesAddress>
    {
        [BotwoonWallDraw.Clear] = new(Bank, 0x930f),
    };

    /// <summary>The address of each <see cref="ChozoStatueDraw"/>.</summary>
    internal static IReadOnlyDictionary<ChozoStatueDraw, SnesAddress> ChozoStatue { get; } = new Dictionary<ChozoStatueDraw, SnesAddress>
    {
        [ChozoStatueDraw.ClearSlopeAccess] = new(Bank, 0x9cc5),
        [ChozoStatueDraw.BlockSlopeAccess] = new(Bank, 0x9d0f),
        [ChozoStatueDraw.LowerNorfairClearedHand] = new(Bank, 0xa2b5),
    };

    /// <summary>The address of each <see cref="ContactCrumbleRestoreDraw"/>.</summary>
    internal static IReadOnlyDictionary<ContactCrumbleRestoreDraw, SnesAddress> ContactCrumbleRestore { get; } = new Dictionary<ContactCrumbleRestoreDraw, SnesAddress>
    {
        [new ContactCrumbleRestoreDraw(BlockArrangement.Horizontal)] = new(Bank, 0xa4a1),
        [new ContactCrumbleRestoreDraw(BlockArrangement.Vertical)] = new(Bank, 0xa4a9),
        [new ContactCrumbleRestoreDraw(BlockArrangement.Square)] = new(Bank, 0xa4b1),
    };

    /// <summary>The address of each <see cref="CrocomireArenaMutation"/>.</summary>
    internal static IReadOnlyDictionary<CrocomireArenaMutation, SnesAddress> CrocomireArena { get; } = new Dictionary<CrocomireArenaMutation, SnesAddress>
    {
        [CrocomireArenaMutation.ClearBridge] = new(Bank, 0x9b5b),
        [CrocomireArenaMutation.CrumbleBridgeBlock] = new(Bank, 0x9b73),
        [CrocomireArenaMutation.ClearBridgeBlock] = new(Bank, 0x9b79),
        [CrocomireArenaMutation.ClearInvisibleWall] = new(Bank, 0x9b7f),
        [CrocomireArenaMutation.CreateInvisibleWall] = new(Bank, 0x9bbb),
    };

    /// <summary>The address of each <see cref="DownwardGateDraw"/>.</summary>
    internal static IReadOnlyDictionary<DownwardGateDraw, SnesAddress> DownwardGate { get; } = new Dictionary<DownwardGateDraw, SnesAddress>
    {
        [DownwardGateDraw.Open] = new(Bank, 0xa517),
        [DownwardGateDraw.QuarterLowered] = new(Bank, 0xa525),
        [DownwardGateDraw.HalfLowered] = new(Bank, 0xa533),
        [DownwardGateDraw.ThreeQuartersLowered] = new(Bank, 0xa541),
        [DownwardGateDraw.FullyLowered] = new(Bank, 0xa54f),
        [DownwardGateDraw.Closed] = new(Bank, 0xa55d),
        [DownwardGateDraw.TriggerBlueLeft] = new(Bank, 0xa5d7),
        [DownwardGateDraw.TriggerBlueRight] = new(Bank, 0xa5e3),
        [DownwardGateDraw.TriggerRedLeft] = new(Bank, 0xa5eb),
        [DownwardGateDraw.TriggerRedRight] = new(Bank, 0xa5f7),
        [DownwardGateDraw.TriggerGreenLeft] = new(Bank, 0xa5ff),
        [DownwardGateDraw.TriggerGreenRight] = new(Bank, 0xa60b),
        [DownwardGateDraw.TriggerYellowLeft] = new(Bank, 0xa613),
        [DownwardGateDraw.TriggerYellowRight] = new(Bank, 0xa61f),
    };

    /// <summary>The address of each <see cref="DraygonCannonDraw"/>.</summary>
    internal static IReadOnlyDictionary<DraygonCannonDraw, SnesAddress> DraygonCannon { get; } = new Dictionary<DraygonCannonDraw, SnesAddress>
    {
        [DraygonCannonDraw.RightShield] = new(Bank, 0x9fcd),
        [DraygonCannonDraw.RightShieldFlash] = new(Bank, 0x9fdd),
        [DraygonCannonDraw.RightSparksFlaring] = new(Bank, 0xa02d),
        [DraygonCannonDraw.RightSparksSpreading] = new(Bank, 0xa03d),
        [DraygonCannonDraw.RightSparksReceding] = new(Bank, 0xa04d),
        [DraygonCannonDraw.RightSparksFlat] = new(Bank, 0xa05d),
        [DraygonCannonDraw.LeftShield] = new(Bank, 0xa0ed),
        [DraygonCannonDraw.LeftShieldFlash] = new(Bank, 0xa101),
        [DraygonCannonDraw.LeftSparksFlaring] = new(Bank, 0xa165),
        [DraygonCannonDraw.LeftSparksSpreading] = new(Bank, 0xa179),
        [DraygonCannonDraw.LeftSparksReceding] = new(Bank, 0xa18d),
        [DraygonCannonDraw.LeftSparksFlat] = new(Bank, 0xa1a1),
    };

    /// <summary>The address of each <see cref="PhantoonDoorDraw"/>.</summary>
    internal static IReadOnlyDictionary<PhantoonDoorDraw, SnesAddress> PhantoonDoor { get; } = new Dictionary<PhantoonDoorDraw, SnesAddress>
    {
        [PhantoonDoorDraw.FightDoor] = new(Bank, 0x93ef),
        [PhantoonDoorDraw.RestoredDoor] = new(Bank, 0x9407),
    };

    /// <summary>The address of each <see cref="ElevatorPlatformDraw"/>.</summary>
    internal static IReadOnlyDictionary<ElevatorPlatformDraw, SnesAddress> ElevatorPlatform { get; } = new Dictionary<ElevatorPlatformDraw, SnesAddress>
    {
        [ElevatorPlatformDraw.Dim] = new(Bank, 0xaa97),
        [ElevatorPlatformDraw.Glowing] = new(Bank, 0xaaaf),
        [ElevatorPlatformDraw.Bright] = new(Bank, 0xaac7),
    };

    /// <summary>The address of each <see cref="EscapeAnimalDraw"/>.</summary>
    internal static IReadOnlyDictionary<EscapeAnimalDraw, SnesAddress> EscapeAnimal { get; } = new Dictionary<EscapeAnimalDraw, SnesAddress>
    {
        [EscapeAnimalDraw.Cracking] = new(Bank, 0x925b),
        [EscapeAnimalDraw.Breaking] = new(Bank, 0x9265),
        [EscapeAnimalDraw.Crumbling] = new(Bank, 0x926f),
        [EscapeAnimalDraw.Blank] = new(Bank, 0x9279),
    };

    /// <summary>The address of each stage of a Kraid room block's crumble (<see cref="KraidRoomObjectDraws.CrumbleDraw"/>).</summary>
    internal static IReadOnlyDictionary<BlockBreakupStage, SnesAddress> KraidCrumble { get; } = new Dictionary<BlockBreakupStage, SnesAddress>
    {
        [BlockBreakupStage.Rock] = new(Bank, 0x9367),
        [BlockBreakupStage.Pieces] = new(Bank, 0x936d),
        [BlockBreakupStage.Dust] = new(Bank, 0x9373),
    };

    /// <summary>The address of each <see cref="KraidRoomDraw"/>.</summary>
    internal static IReadOnlyDictionary<KraidRoomDraw, SnesAddress> KraidRoom { get; } = new Dictionary<KraidRoomDraw, SnesAddress>
    {
        [KraidRoomDraw.CeilingBackgroundLeftEdge] = new(Bank, 0x9379),
        [KraidRoomDraw.CeilingBackgroundOddColumn] = new(Bank, 0x937f),
        [KraidRoomDraw.CeilingBackgroundEvenColumn] = new(Bank, 0x9385),
        [KraidRoomDraw.SpikeBackgroundLeftColumn] = new(Bank, 0x9391),
        [KraidRoomDraw.SpikeBackgroundRightColumn] = new(Bank, 0x9397),
        [KraidRoomDraw.ClearCeiling] = new(Bank, 0x939d),
        [KraidRoomDraw.ClearSpikes] = new(Bank, 0x93bf),
    };

    /// <summary>The address of each <see cref="MotherBrainEscapeGateDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainEscapeGateDraw, SnesAddress> MotherBrainEscapeGate { get; } = new Dictionary<MotherBrainEscapeGateDraw, SnesAddress>
    {
        [MotherBrainEscapeGateDraw.Open] = new(Bank, 0x9473),
        [MotherBrainEscapeGateDraw.HalfClosed] = new(Bank, 0x947f),
        [MotherBrainEscapeGateDraw.Closed] = new(Bank, 0x948b),
    };

    /// <summary>The address of each <see cref="MotherBrainFakeDeathDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainFakeDeathDraw, SnesAddress> MotherBrainFakeDeath { get; } = new Dictionary<MotherBrainFakeDeathDraw, SnesAddress>
    {
        [MotherBrainFakeDeathDraw.FillWall] = new(Bank, 0x94a3),
        [MotherBrainFakeDeathDraw.EscapeDoor] = new(Bank, 0x94b1),
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = new(Bank, 0x96a9),
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = new(Bank, 0x96b1),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = new(Bank, 0x96bf),
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = new(Bank, 0x96cb),
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = new(Bank, 0x96ef),
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = new(Bank, 0x9703),
    };

    /// <summary>The address of each <see cref="MotherBrainGlassDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassDraw, SnesAddress> MotherBrainGlass { get; } = new Dictionary<MotherBrainGlassDraw, SnesAddress>
    {
        [MotherBrainGlassDraw.Intact] = new(Bank, 0x9717),
        [MotherBrainGlassDraw.PaneCracked] = new(Bank, 0x971d),
        [MotherBrainGlassDraw.PaneCrackedThrough] = new(Bank, 0x9731),
        [MotherBrainGlassDraw.PaneTransition] = new(Bank, 0x9745),
        [MotherBrainGlassDraw.PaneShifted] = new(Bank, 0x974f),
        [MotherBrainGlassDraw.PaneShiftedCrumbling] = new(Bank, 0x9769),
        [MotherBrainGlassDraw.PaneShiftedEmptying] = new(Bank, 0x9781),
        [MotherBrainGlassDraw.RimBuckling] = new(Bank, 0x978f),
        [MotherBrainGlassDraw.RimBursting] = new(Bank, 0x97b7),
        [MotherBrainGlassDraw.RimGone] = new(Bank, 0x97e7),
        [MotherBrainGlassDraw.Cleared] = new(Bank, 0x9817),
    };

    /// <summary>The address of each <see cref="NoobTubeDraw"/>.</summary>
    internal static IReadOnlyDictionary<NoobTubeDraw, SnesAddress> NoobTube { get; } = new Dictionary<NoobTubeDraw, SnesAddress>
    {
        [NoobTubeDraw.Intact] = new(Bank, 0x98d1),
        [NoobTubeDraw.Cracking] = new(Bank, 0x98d7),
        [NoobTubeDraw.Opened] = new(Bank, 0x98dd),
        [NoobTubeDraw.UpperRowsOpen] = new(Bank, 0x9991),
        [NoobTubeDraw.LowerRowsOpen] = new(Bank, 0x99e5),
    };

    /// <summary>The address of each <see cref="RoomObjectBombedRevealDraw"/>.</summary>
    internal static IReadOnlyDictionary<RoomObjectBombedRevealDraw, SnesAddress> RoomObjectBombedReveal { get; } = new Dictionary<RoomObjectBombedRevealDraw, SnesAddress>
    {
        [RoomObjectBombedRevealDraw.CrumbleSingle] = new(Bank, 0xa49b),
        [RoomObjectBombedRevealDraw.PowerBomb] = new(Bank, 0xa4e7),
        [RoomObjectBombedRevealDraw.SuperMissile] = new(Bank, 0xa4ed),
    };

    /// <summary>The address of each <see cref="SamusEaterDraw"/>.</summary>
    internal static IReadOnlyDictionary<SamusEaterDraw, SnesAddress> SamusEater { get; } = new Dictionary<SamusEaterDraw, SnesAddress>
    {
        [SamusEaterDraw.FloorIdle] = new(Bank, 0x9e0d),
        [SamusEaterDraw.FloorTeethPeeking] = new(Bank, 0x9e45),
        [SamusEaterDraw.FloorTeethBared] = new(Bank, 0x9e61),
        [SamusEaterDraw.FloorFangsOut] = new(Bank, 0x9e7d),
        [SamusEaterDraw.CeilingIdle] = new(Bank, 0x9e99),
        [SamusEaterDraw.CeilingTeethPeeking] = new(Bank, 0x9ed1),
        [SamusEaterDraw.CeilingTeethBared] = new(Bank, 0x9eed),
        [SamusEaterDraw.CeilingFangsOut] = new(Bank, 0x9f09),
    };

    /// <summary>The address of each <see cref="SporeSpawnCeilingDraw"/>.</summary>
    internal static IReadOnlyDictionary<SporeSpawnCeilingDraw, SnesAddress> SporeSpawnCeiling { get; } = new Dictionary<SporeSpawnCeilingDraw, SnesAddress>
    {
        [SporeSpawnCeilingDraw.Clear] = new(Bank, 0x9413),
        [SporeSpawnCeilingDraw.CeilingCracked] = new(Bank, 0x9423),
        [SporeSpawnCeilingDraw.CeilingCrumbling] = new(Bank, 0x9433),
        [SporeSpawnCeilingDraw.CeilingCrumbled] = new(Bank, 0x9443),
    };

    /// <summary>The address of each <see cref="StationDraw"/>.</summary>
    internal static IReadOnlyDictionary<StationDraw, SnesAddress> Station { get; } = new Dictionary<StationDraw, SnesAddress>
    {
        [StationDraw.MapDim] = new(Bank, 0x9f25),
        [StationDraw.MapLit] = new(Bank, 0x9f31),
        [StationDraw.MapBright] = new(Bank, 0x9f3d),
        [StationDraw.MapRightRetracted] = new(Bank, 0x9f49),
        [StationDraw.MapRightExtended] = new(Bank, 0x9f55),
        [StationDraw.MapLeftRetracted] = new(Bank, 0x9f5b),
        [StationDraw.MapLeftExtended] = new(Bank, 0x9f67),
        [StationDraw.EnergyDim] = new(Bank, 0x9f6d),
        [StationDraw.EnergyLit] = new(Bank, 0x9f79),
        [StationDraw.EnergyBright] = new(Bank, 0x9f85),
        [StationDraw.MissileDim] = new(Bank, 0x9f91),
        [StationDraw.MissileLit] = new(Bank, 0x9f9d),
        [StationDraw.MissileBright] = new(Bank, 0x9fa9),
        [StationDraw.ResourceRightRetracted] = new(Bank, 0x9fb5),
        [StationDraw.ResourceRightExtended] = new(Bank, 0x9fbb),
        [StationDraw.ResourceLeftRetracted] = new(Bank, 0x9fc1),
        [StationDraw.ResourceLeftExtended] = new(Bank, 0x9fc7),
        [StationDraw.SaveIdle] = new(Bank, 0x9a3f),
        [StationDraw.SaveLit] = new(Bank, 0x9a9f),
        [StationDraw.SaveDark] = new(Bank, 0x9a6f),
    };

    /// <summary>The address of each <see cref="TourianAccessDraw"/>.</summary>
    internal static IReadOnlyDictionary<TourianAccessDraw, SnesAddress> TourianAccess { get; } = new Dictionary<TourianAccessDraw, SnesAddress>
    {
        [TourianAccessDraw.EmptyRow] = new(Bank, 0x9297),
        [TourianAccessDraw.Clear] = new(Bank, 0x92c7),
    };

    /// <summary>The address of each drawn stage of a Tourian access row's crumble (<see cref="TourianAccessDraws.CrumbleDraw"/>).</summary>
    internal static IReadOnlyDictionary<BlockBreakupStage, SnesAddress> TourianAccessCrumble { get; } = new Dictionary<BlockBreakupStage, SnesAddress>
    {
        [BlockBreakupStage.Rock] = new(Bank, 0x92a3),
        [BlockBreakupStage.Pieces] = new(Bank, 0x92af),
        [BlockBreakupStage.Dust] = new(Bank, 0x92bb),
    };

    /// <summary>The top row of Mother Brain's mural; each row below it follows thirty bytes on.</summary>
    internal static readonly SnesAddress MotherBrainMuralTopRow = new(Bank, 0x9505);

    /// <summary>The single-block shot-block breakup frames, six bytes apart.</summary>
    internal static readonly SnesAddress SingleBreakupFirst = new(Bank, 0xa345);

    /// <summary>The horizontal-pair breakup frames, eight bytes apart.</summary>
    internal static readonly SnesAddress HorizontalBreakupFirst = new(Bank, 0xa35d);

    /// <summary>The vertical-pair breakup frames, eight bytes apart.</summary>
    internal static readonly SnesAddress VerticalBreakupFirst = new(Bank, 0xa37d);

    /// <summary>The square breakup frames, sixteen bytes apart.</summary>
    internal static readonly SnesAddress SquareBreakupFirst = new(Bank, 0xa39d);

    /// <summary>The horizontal shot-block restoration.</summary>
    internal static readonly SnesAddress ShotBlockRestoreHorizontal = new(Bank, 0xa47b);

    /// <summary>The vertical shot-block restoration.</summary>
    internal static readonly SnesAddress ShotBlockRestoreVertical = new(Bank, 0xa483);

    /// <summary>The square shot-block restoration.</summary>
    internal static readonly SnesAddress ShotBlockRestoreSquare = new(Bank, 0xa48b);

    /// <summary>The five Grapple-block draws, six bytes apart.</summary>
    internal static readonly SnesAddress GrappleBlockFirst = new(Bank, 0xa4f9);

    /// <summary>The address of each <see cref="CollectibleDraw"/>: one six-byte one-block record per frame.</summary>
    internal static IReadOnlyDictionary<CollectibleDraw, SnesAddress> Collectible { get; } = new Dictionary<CollectibleDraw, SnesAddress>
    {
        [CollectibleDraw.Empty] = new(Bank, 0xa2b5),
        [CollectibleDraw.ChozoOrbDim] = new(Bank, 0xa2c7),
        [CollectibleDraw.ChozoOrbGlowing] = new(Bank, 0xa2cd),
        [CollectibleDraw.ChozoOrbBright] = new(Bank, 0xa2d3),
        [CollectibleDraw.ChozoOrbBurst] = new(Bank, 0xa2d9),
        [CollectibleDraw.EnergyTankBright] = new(Bank, 0xa2df),
        [CollectibleDraw.EnergyTankDim] = new(Bank, 0xa2e5),
        [CollectibleDraw.MissileTankDim] = new(Bank, 0xa2eb),
        [CollectibleDraw.MissileTankBright] = new(Bank, 0xa2f1),
        [CollectibleDraw.SuperMissileTankDim] = new(Bank, 0xa2f7),
        [CollectibleDraw.SuperMissileTankBright] = new(Bank, 0xa2fd),
        [CollectibleDraw.PowerBombTankBright] = new(Bank, 0xa303),
        [CollectibleDraw.PowerBombTankDim] = new(Bank, 0xa309),
        [CollectibleDraw.SolidPairPrimaryLook] = new(Bank, 0xa30f),
        [CollectibleDraw.SolidPairAlternateLook] = new(Bank, 0xa315),
        [CollectibleDraw.FirstDoorPairPrimaryLook] = new(Bank, 0xa31b),
        [CollectibleDraw.FirstDoorPairAlternateLook] = new(Bank, 0xa321),
        [CollectibleDraw.SecondDoorPairPrimaryLook] = new(Bank, 0xa327),
        [CollectibleDraw.SecondDoorPairAlternateLook] = new(Bank, 0xa32d),
        [CollectibleDraw.ThirdDoorPairPrimaryLook] = new(Bank, 0xa333),
        [CollectibleDraw.ThirdDoorPairAlternateLook] = new(Bank, 0xa339),
        [CollectibleDraw.ShotBlockIntact] = new(Bank, 0xa3dd),
        [CollectibleDraw.ShotBlockCracking] = new(Bank, 0xa3e3),
        [CollectibleDraw.ShotBlockDust] = new(Bank, 0xa3e9),
    };

    /// <summary>The dynamic-item first-frame selector table.</summary>
    internal static readonly SnesAddress CollectibleDynamicFrame0Table = new(Bank, 0xe05f);

    /// <summary>The dynamic-item second-frame selector table.</summary>
    internal static readonly SnesAddress CollectibleDynamicFrame1Table = new(Bank, 0xe077);

    /// <summary>The left-facing cleared door opening; the other facings follow twelve bytes apart.</summary>
    internal static readonly SnesAddress DoorClearLeft = new(Bank, 0xa677);

    /// <summary>The left-facing grey cap; frames twelve bytes apart, facings 48 bytes apart.</summary>
    internal static readonly SnesAddress GreyDoorLeft = new(Bank, 0xa6a7);

    /// <summary>The left-facing yellow cap; colours 192 bytes apart.</summary>
    internal static readonly SnesAddress ColoredDoorYellowLeft = new(Bank, 0xa767);

    /// <summary>The left-facing blue-cap alias; each facing's alias and four frames span sixty bytes.</summary>
    internal static readonly SnesAddress BlueDoorLeftAlias = new(Bank, 0xa9a7);

    /// <summary>The left eye door's eye frames, eight bytes apart.</summary>
    internal static readonly SnesAddress EyeDoorLeftEyeFirst = new(Bank, 0x9c03);

    /// <summary>The left eye door's middle frames, six bytes apart.</summary>
    internal static readonly SnesAddress EyeDoorLeftMiddleFirst = new(Bank, 0x9c2b);

    /// <summary>The left eye door's bottom frames, six bytes apart.</summary>
    internal static readonly SnesAddress EyeDoorLeftBottomFirst = new(Bank, 0x9c3d);

    /// <summary>The left eye door's opening clear.</summary>
    internal static readonly SnesAddress EyeDoorMirroredOpeningClear = new(Bank, 0x9bf7);

    /// <summary>How far the right eye door's draws follow the left's.</summary>
    internal const int EyeDoorRightOffset = 0x0058;

    /// <summary>The Crateria escape passage's clear.</summary>
    internal static readonly SnesAddress CrateriaPassageClearPair = new(Bank, 0x9253);

    /// <summary>The Old Tourian escape shaft wall's opening.</summary>
    internal static readonly SnesAddress OldTourianShaftWallOpen = new(Bank, 0x9283);

    /// <summary>The Maridia elevatube block, shared with Kraid's first ceiling crumble.</summary>
    internal static readonly SnesAddress MaridiaElevatubeBlock = new(Bank, 0x9367);

    /// <summary>The Speed Booster block's bomb reveal.</summary>
    internal static readonly SnesAddress SpeedBoosterBombReveal = new(Bank, 0xa4f3);
}
