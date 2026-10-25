using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where the cartridge stores each PLM draw (#1402): the import pipeline's private knowledge, used to read and verify
/// a draw's level words when its artwork is extracted. Core identifies draws only semantically.
/// </summary>
internal static class PlmDrawSourceDefinitions
{
    /// <summary>Bank $84, where every PLM draw is stored.</summary>
    private const byte Bank = 0x84;

    /// <summary>The address of each <see cref="BombBlockRestoreDraw"/>.</summary>
    internal static IReadOnlyDictionary<BombBlockRestoreDraw, SnesAddress> BombBlockRestore { get; } = new Dictionary<BombBlockRestoreDraw, SnesAddress>
    {
        [BombBlockRestoreDraw.Horizontal] = new(Bank, 0xa4c7),
        [BombBlockRestoreDraw.Vertical] = new(Bank, 0xa4cf),
        [BombBlockRestoreDraw.Square] = new(Bank, 0xa4d7),
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
        [ContactCrumbleRestoreDraw.Horizontal] = new(Bank, 0xa4a1),
        [ContactCrumbleRestoreDraw.Vertical] = new(Bank, 0xa4a9),
        [ContactCrumbleRestoreDraw.Square] = new(Bank, 0xa4b1),
    };

    /// <summary>The address of each <see cref="CrocomireArenaDraw"/>.</summary>
    internal static IReadOnlyDictionary<CrocomireArenaDraw, SnesAddress> CrocomireArena { get; } = new Dictionary<CrocomireArenaDraw, SnesAddress>
    {
        [CrocomireArenaDraw.ClearBridge] = new(Bank, 0x9b5b),
        [CrocomireArenaDraw.CrumbleBridgeBlock] = new(Bank, 0x9b73),
        [CrocomireArenaDraw.ClearBridgeBlock] = new(Bank, 0x9b79),
        [CrocomireArenaDraw.ClearInvisibleWall] = new(Bank, 0x9b7f),
        [CrocomireArenaDraw.CreateInvisibleWall] = new(Bank, 0x9bbb),
    };

    /// <summary>The address of each <see cref="DownwardGateDraw"/>.</summary>
    internal static IReadOnlyDictionary<DownwardGateDraw, SnesAddress> DownwardGate { get; } = new Dictionary<DownwardGateDraw, SnesAddress>
    {
        [DownwardGateDraw.Open] = new(Bank, 0xa517),
        [DownwardGateDraw.Lowered1] = new(Bank, 0xa525),
        [DownwardGateDraw.Lowered2] = new(Bank, 0xa533),
        [DownwardGateDraw.Lowered3] = new(Bank, 0xa541),
        [DownwardGateDraw.Lowered4] = new(Bank, 0xa54f),
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
        [DraygonCannonDraw.RightShieldA] = new(Bank, 0x9fcd),
        [DraygonCannonDraw.RightShieldB] = new(Bank, 0x9fdd),
        [DraygonCannonDraw.RightDamagedA] = new(Bank, 0xa02d),
        [DraygonCannonDraw.RightDamagedB] = new(Bank, 0xa03d),
        [DraygonCannonDraw.RightDamagedC] = new(Bank, 0xa04d),
        [DraygonCannonDraw.RightDamagedD] = new(Bank, 0xa05d),
        [DraygonCannonDraw.LeftShieldA] = new(Bank, 0xa0ed),
        [DraygonCannonDraw.LeftShieldB] = new(Bank, 0xa101),
        [DraygonCannonDraw.LeftDamagedA] = new(Bank, 0xa165),
        [DraygonCannonDraw.LeftDamagedB] = new(Bank, 0xa179),
        [DraygonCannonDraw.LeftDamagedC] = new(Bank, 0xa18d),
        [DraygonCannonDraw.LeftDamagedD] = new(Bank, 0xa1a1),
    };

    /// <summary>The address of each <see cref="ElevatorPlatformDraw"/>.</summary>
    internal static IReadOnlyDictionary<ElevatorPlatformDraw, SnesAddress> ElevatorPlatform { get; } = new Dictionary<ElevatorPlatformDraw, SnesAddress>
    {
        [ElevatorPlatformDraw.First] = new(Bank, 0xaa97),
        [ElevatorPlatformDraw.Second] = new(Bank, 0xaaaf),
        [ElevatorPlatformDraw.Third] = new(Bank, 0xaac7),
    };

    /// <summary>The address of each <see cref="EscapeAnimalDraw"/>.</summary>
    internal static IReadOnlyDictionary<EscapeAnimalDraw, SnesAddress> EscapeAnimal { get; } = new Dictionary<EscapeAnimalDraw, SnesAddress>
    {
        [EscapeAnimalDraw.Frame0] = new(Bank, 0x925b),
        [EscapeAnimalDraw.Frame1] = new(Bank, 0x9265),
        [EscapeAnimalDraw.Frame2] = new(Bank, 0x926f),
        [EscapeAnimalDraw.Blank] = new(Bank, 0x9279),
    };

    /// <summary>The address of each <see cref="KraidRoomDraw"/>.</summary>
    internal static IReadOnlyDictionary<KraidRoomDraw, SnesAddress> KraidRoom { get; } = new Dictionary<KraidRoomDraw, SnesAddress>
    {
        [KraidRoomDraw.CrumbleFirst] = new(Bank, 0x9367),
        [KraidRoomDraw.CrumbleSecond] = new(Bank, 0x936d),
        [KraidRoomDraw.CrumbleThird] = new(Bank, 0x9373),
        [KraidRoomDraw.CeilingBackground1] = new(Bank, 0x9379),
        [KraidRoomDraw.CeilingBackground2] = new(Bank, 0x937f),
        [KraidRoomDraw.CeilingBackground3] = new(Bank, 0x9385),
        [KraidRoomDraw.SpikeFirst] = new(Bank, 0x9391),
        [KraidRoomDraw.SpikeSecond] = new(Bank, 0x9397),
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
        [MotherBrainFakeDeathDraw.BackgroundRow2] = new(Bank, 0x9505),
        [MotherBrainFakeDeathDraw.BackgroundRow3] = new(Bank, 0x9523),
        [MotherBrainFakeDeathDraw.BackgroundRow4] = new(Bank, 0x9541),
        [MotherBrainFakeDeathDraw.BackgroundRow5] = new(Bank, 0x955f),
        [MotherBrainFakeDeathDraw.BackgroundRow6] = new(Bank, 0x957d),
        [MotherBrainFakeDeathDraw.BackgroundRow7] = new(Bank, 0x959b),
        [MotherBrainFakeDeathDraw.BackgroundRow8] = new(Bank, 0x95b9),
        [MotherBrainFakeDeathDraw.BackgroundRow9] = new(Bank, 0x95d7),
        [MotherBrainFakeDeathDraw.BackgroundRowA] = new(Bank, 0x95f5),
        [MotherBrainFakeDeathDraw.BackgroundRowB] = new(Bank, 0x9613),
        [MotherBrainFakeDeathDraw.BackgroundRowC] = new(Bank, 0x9631),
        [MotherBrainFakeDeathDraw.BackgroundRowD] = new(Bank, 0x964f),
        [MotherBrainFakeDeathDraw.BackgroundRowEUnused] = new(Bank, 0x966d),
        [MotherBrainFakeDeathDraw.BackgroundRowFUnused] = new(Bank, 0x968b),
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
        [MotherBrainGlassDraw.Initial] = new(Bank, 0x9717),
        [MotherBrainGlassDraw.PaneDamage1] = new(Bank, 0x971d),
        [MotherBrainGlassDraw.PaneDamage2] = new(Bank, 0x9731),
        [MotherBrainGlassDraw.PaneTransition] = new(Bank, 0x9745),
        [MotherBrainGlassDraw.ShiftedPane1] = new(Bank, 0x974f),
        [MotherBrainGlassDraw.ShiftedPane2] = new(Bank, 0x9769),
        [MotherBrainGlassDraw.ShiftedPane3] = new(Bank, 0x9781),
        [MotherBrainGlassDraw.Shatter1] = new(Bank, 0x978f),
        [MotherBrainGlassDraw.Shatter2] = new(Bank, 0x97b7),
        [MotherBrainGlassDraw.Shatter3] = new(Bank, 0x97e7),
        [MotherBrainGlassDraw.Cleared] = new(Bank, 0x9817),
    };

    /// <summary>The address of each <see cref="NoobTubeDraw"/>.</summary>
    internal static IReadOnlyDictionary<NoobTubeDraw, SnesAddress> NoobTube { get; } = new Dictionary<NoobTubeDraw, SnesAddress>
    {
        [NoobTubeDraw.Intact] = new(Bank, 0x98d1),
        [NoobTubeDraw.Damaged] = new(Bank, 0x98d7),
        [NoobTubeDraw.Opened] = new(Bank, 0x98dd),
        [NoobTubeDraw.Cleared] = new(Bank, 0x98e3),
        [NoobTubeDraw.BrokenLate] = new(Bank, 0x9953),
        [NoobTubeDraw.OpenedRows] = new(Bank, 0x9991),
        [NoobTubeDraw.BrokenFull] = new(Bank, 0x99e5),
    };

    /// <summary>The address of each <see cref="RoomPlmBombedRevealDraw"/>.</summary>
    internal static IReadOnlyDictionary<RoomPlmBombedRevealDraw, SnesAddress> RoomPlmBombedReveal { get; } = new Dictionary<RoomPlmBombedRevealDraw, SnesAddress>
    {
        [RoomPlmBombedRevealDraw.CrumbleSingle] = new(Bank, 0xa49b),
        [RoomPlmBombedRevealDraw.PowerBomb] = new(Bank, 0xa4e7),
        [RoomPlmBombedRevealDraw.SuperMissile] = new(Bank, 0xa4ed),
    };

    /// <summary>The address of each <see cref="SamusEaterDraw"/>.</summary>
    internal static IReadOnlyDictionary<SamusEaterDraw, SnesAddress> SamusEater { get; } = new Dictionary<SamusEaterDraw, SnesAddress>
    {
        [SamusEaterDraw.FloorIdle] = new(Bank, 0x9e0d),
        [SamusEaterDraw.FloorChew1] = new(Bank, 0x9e45),
        [SamusEaterDraw.FloorChew2] = new(Bank, 0x9e61),
        [SamusEaterDraw.FloorChew3] = new(Bank, 0x9e7d),
        [SamusEaterDraw.CeilingIdle] = new(Bank, 0x9e99),
        [SamusEaterDraw.CeilingChew1] = new(Bank, 0x9ed1),
        [SamusEaterDraw.CeilingChew2] = new(Bank, 0x9eed),
        [SamusEaterDraw.CeilingChew3] = new(Bank, 0x9f09),
    };

    /// <summary>The address of each <see cref="SporeSpawnCeilingDraw"/>.</summary>
    internal static IReadOnlyDictionary<SporeSpawnCeilingDraw, SnesAddress> SporeSpawnCeiling { get; } = new Dictionary<SporeSpawnCeilingDraw, SnesAddress>
    {
        [SporeSpawnCeilingDraw.Clear] = new(Bank, 0x9413),
        [SporeSpawnCeilingDraw.CrumbleFirst] = new(Bank, 0x9423),
        [SporeSpawnCeilingDraw.CrumbleSecond] = new(Bank, 0x9433),
        [SporeSpawnCeilingDraw.CrumbleThird] = new(Bank, 0x9443),
    };

    /// <summary>The address of each <see cref="StationDraw"/>.</summary>
    internal static IReadOnlyDictionary<StationDraw, SnesAddress> Station { get; } = new Dictionary<StationDraw, SnesAddress>
    {
        [StationDraw.MapFrame0] = new(Bank, 0x9f25),
        [StationDraw.MapFrame1] = new(Bank, 0x9f31),
        [StationDraw.MapFrame2] = new(Bank, 0x9f3d),
        [StationDraw.MapRightRetracted] = new(Bank, 0x9f49),
        [StationDraw.MapRightExtended] = new(Bank, 0x9f55),
        [StationDraw.MapLeftRetracted] = new(Bank, 0x9f5b),
        [StationDraw.MapLeftExtended] = new(Bank, 0x9f67),
        [StationDraw.EnergyFrame0] = new(Bank, 0x9f6d),
        [StationDraw.EnergyFrame1] = new(Bank, 0x9f79),
        [StationDraw.EnergyFrame2] = new(Bank, 0x9f85),
        [StationDraw.MissileFrame0] = new(Bank, 0x9f91),
        [StationDraw.MissileFrame1] = new(Bank, 0x9f9d),
        [StationDraw.MissileFrame2] = new(Bank, 0x9fa9),
        [StationDraw.ResourceRightRetracted] = new(Bank, 0x9fb5),
        [StationDraw.ResourceRightExtended] = new(Bank, 0x9fbb),
        [StationDraw.ResourceLeftRetracted] = new(Bank, 0x9fc1),
        [StationDraw.ResourceLeftExtended] = new(Bank, 0x9fc7),
        [StationDraw.SaveIdle] = new(Bank, 0x9a3f),
        [StationDraw.SaveActive] = new(Bank, 0x9a9f),
        [StationDraw.SaveAlternate] = new(Bank, 0x9a6f),
    };

    /// <summary>The address of each <see cref="TourianAccessDraw"/>.</summary>
    internal static IReadOnlyDictionary<TourianAccessDraw, SnesAddress> TourianAccess { get; } = new Dictionary<TourianAccessDraw, SnesAddress>
    {
        [TourianAccessDraw.EmptyRow] = new(Bank, 0x9297),
        [TourianAccessDraw.CrumbleFirst] = new(Bank, 0x92a3),
        [TourianAccessDraw.CrumbleSecond] = new(Bank, 0x92af),
        [TourianAccessDraw.CrumbleThird] = new(Bank, 0x92bb),
        [TourianAccessDraw.Clear] = new(Bank, 0x92c7),
    };

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

    /// <summary>The empty-item draw.</summary>
    internal static readonly SnesAddress CollectibleEmpty = new(Bank, 0xa2b5);

    /// <summary>The Chozo orb frames, six bytes apart.</summary>
    internal static readonly SnesAddress CollectibleOrbFirst = new(Bank, 0xa2c7);

    /// <summary>The Chozo orb burst.</summary>
    internal static readonly SnesAddress CollectibleOrbBurst = new(Bank, 0xa2d9);

    /// <summary>The tank frames, six bytes apart.</summary>
    internal static readonly SnesAddress CollectibleTankFirst = new(Bank, 0xa2df);

    /// <summary>The dynamically assigned item frames, six bytes apart.</summary>
    internal static readonly SnesAddress CollectibleDynamicFirst = new(Bank, 0xa30f);

    /// <summary>The shot-block reveal frames, six bytes apart.</summary>
    internal static readonly SnesAddress CollectibleShotRevealFirst = new(Bank, 0xa3dd);

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
