using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where the cartridge stores each PLM draw (#1402): the import pipeline's private knowledge, used to read and verify
/// a draw's level words when its artwork is extracted. Core identifies draws only semantically.
/// </summary>
internal static class PlmDrawAddresses
{
    /// <summary>The address of each <see cref="BombBlockRestoreDraw"/>.</summary>
    internal static IReadOnlyDictionary<BombBlockRestoreDraw, ushort> BombBlockRestore { get; } = new Dictionary<BombBlockRestoreDraw, ushort>
    {
        [BombBlockRestoreDraw.Horizontal] = 0xa4c7,
        [BombBlockRestoreDraw.Vertical] = 0xa4cf,
        [BombBlockRestoreDraw.Square] = 0xa4d7,
    };

    /// <summary>The address of each <see cref="BombTorizoHandDraw"/>.</summary>
    internal static IReadOnlyDictionary<BombTorizoHandDraw, ushort> BombTorizoHand { get; } = new Dictionary<BombTorizoHandDraw, ushort>
    {
        [BombTorizoHandDraw.Intact] = 0x9877,
        [BombTorizoHandDraw.Cleared] = 0x989d,
    };

    /// <summary>The address of each <see cref="BotwoonWallDraw"/>.</summary>
    internal static IReadOnlyDictionary<BotwoonWallDraw, ushort> BotwoonWall { get; } = new Dictionary<BotwoonWallDraw, ushort>
    {
        [BotwoonWallDraw.Clear] = 0x930f,
    };

    /// <summary>The address of each <see cref="ChozoStatueDraw"/>.</summary>
    internal static IReadOnlyDictionary<ChozoStatueDraw, ushort> ChozoStatue { get; } = new Dictionary<ChozoStatueDraw, ushort>
    {
        [ChozoStatueDraw.ClearSlopeAccess] = 0x9cc5,
        [ChozoStatueDraw.BlockSlopeAccess] = 0x9d0f,
        [ChozoStatueDraw.LowerNorfairClearedHand] = 0xa2b5,
    };

    /// <summary>The address of each <see cref="ContactCrumbleRestoreDraw"/>.</summary>
    internal static IReadOnlyDictionary<ContactCrumbleRestoreDraw, ushort> ContactCrumbleRestore { get; } = new Dictionary<ContactCrumbleRestoreDraw, ushort>
    {
        [ContactCrumbleRestoreDraw.Horizontal] = 0xa4a1,
        [ContactCrumbleRestoreDraw.Vertical] = 0xa4a9,
        [ContactCrumbleRestoreDraw.Square] = 0xa4b1,
    };

    /// <summary>The address of each <see cref="CrocomireArenaDraw"/>.</summary>
    internal static IReadOnlyDictionary<CrocomireArenaDraw, ushort> CrocomireArena { get; } = new Dictionary<CrocomireArenaDraw, ushort>
    {
        [CrocomireArenaDraw.ClearBridge] = 0x9b5b,
        [CrocomireArenaDraw.CrumbleBridgeBlock] = 0x9b73,
        [CrocomireArenaDraw.ClearBridgeBlock] = 0x9b79,
        [CrocomireArenaDraw.ClearInvisibleWall] = 0x9b7f,
        [CrocomireArenaDraw.CreateInvisibleWall] = 0x9bbb,
    };

    /// <summary>The address of each <see cref="DownwardGateDraw"/>.</summary>
    internal static IReadOnlyDictionary<DownwardGateDraw, ushort> DownwardGate { get; } = new Dictionary<DownwardGateDraw, ushort>
    {
        [DownwardGateDraw.Open] = 0xa517,
        [DownwardGateDraw.Lowered1] = 0xa525,
        [DownwardGateDraw.Lowered2] = 0xa533,
        [DownwardGateDraw.Lowered3] = 0xa541,
        [DownwardGateDraw.Lowered4] = 0xa54f,
        [DownwardGateDraw.Closed] = 0xa55d,
        [DownwardGateDraw.TriggerBlueLeft] = 0xa5d7,
        [DownwardGateDraw.TriggerBlueRight] = 0xa5e3,
        [DownwardGateDraw.TriggerRedLeft] = 0xa5eb,
        [DownwardGateDraw.TriggerRedRight] = 0xa5f7,
        [DownwardGateDraw.TriggerGreenLeft] = 0xa5ff,
        [DownwardGateDraw.TriggerGreenRight] = 0xa60b,
        [DownwardGateDraw.TriggerYellowLeft] = 0xa613,
        [DownwardGateDraw.TriggerYellowRight] = 0xa61f,
    };

    /// <summary>The address of each <see cref="DraygonCannonDraw"/>.</summary>
    internal static IReadOnlyDictionary<DraygonCannonDraw, ushort> DraygonCannon { get; } = new Dictionary<DraygonCannonDraw, ushort>
    {
        [DraygonCannonDraw.RightShieldA] = 0x9fcd,
        [DraygonCannonDraw.RightShieldB] = 0x9fdd,
        [DraygonCannonDraw.RightDamagedA] = 0xa02d,
        [DraygonCannonDraw.RightDamagedB] = 0xa03d,
        [DraygonCannonDraw.RightDamagedC] = 0xa04d,
        [DraygonCannonDraw.RightDamagedD] = 0xa05d,
        [DraygonCannonDraw.LeftShieldA] = 0xa0ed,
        [DraygonCannonDraw.LeftShieldB] = 0xa101,
        [DraygonCannonDraw.LeftDamagedA] = 0xa165,
        [DraygonCannonDraw.LeftDamagedB] = 0xa179,
        [DraygonCannonDraw.LeftDamagedC] = 0xa18d,
        [DraygonCannonDraw.LeftDamagedD] = 0xa1a1,
    };

    /// <summary>The address of each <see cref="ElevatorPlatformDraw"/>.</summary>
    internal static IReadOnlyDictionary<ElevatorPlatformDraw, ushort> ElevatorPlatform { get; } = new Dictionary<ElevatorPlatformDraw, ushort>
    {
        [ElevatorPlatformDraw.First] = 0xaa97,
        [ElevatorPlatformDraw.Second] = 0xaaaf,
        [ElevatorPlatformDraw.Third] = 0xaac7,
    };

    /// <summary>The address of each <see cref="EscapeAnimalDraw"/>.</summary>
    internal static IReadOnlyDictionary<EscapeAnimalDraw, ushort> EscapeAnimal { get; } = new Dictionary<EscapeAnimalDraw, ushort>
    {
        [EscapeAnimalDraw.Frame0] = 0x925b,
        [EscapeAnimalDraw.Frame1] = 0x9265,
        [EscapeAnimalDraw.Frame2] = 0x926f,
        [EscapeAnimalDraw.Blank] = 0x9279,
    };

    /// <summary>The address of each <see cref="KraidRoomDraw"/>.</summary>
    internal static IReadOnlyDictionary<KraidRoomDraw, ushort> KraidRoom { get; } = new Dictionary<KraidRoomDraw, ushort>
    {
        [KraidRoomDraw.CrumbleFirst] = 0x9367,
        [KraidRoomDraw.CrumbleSecond] = 0x936d,
        [KraidRoomDraw.CrumbleThird] = 0x9373,
        [KraidRoomDraw.CeilingBackground1] = 0x9379,
        [KraidRoomDraw.CeilingBackground2] = 0x937f,
        [KraidRoomDraw.CeilingBackground3] = 0x9385,
        [KraidRoomDraw.SpikeFirst] = 0x9391,
        [KraidRoomDraw.SpikeSecond] = 0x9397,
        [KraidRoomDraw.ClearCeiling] = 0x939d,
        [KraidRoomDraw.ClearSpikes] = 0x93bf,
    };

    /// <summary>The address of each <see cref="MotherBrainEscapeGateDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainEscapeGateDraw, ushort> MotherBrainEscapeGate { get; } = new Dictionary<MotherBrainEscapeGateDraw, ushort>
    {
        [MotherBrainEscapeGateDraw.Open] = 0x9473,
        [MotherBrainEscapeGateDraw.HalfClosed] = 0x947f,
        [MotherBrainEscapeGateDraw.Closed] = 0x948b,
    };

    /// <summary>The address of each <see cref="MotherBrainFakeDeathDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainFakeDeathDraw, ushort> MotherBrainFakeDeath { get; } = new Dictionary<MotherBrainFakeDeathDraw, ushort>
    {
        [MotherBrainFakeDeathDraw.FillWall] = 0x94a3,
        [MotherBrainFakeDeathDraw.EscapeDoor] = 0x94b1,
        [MotherBrainFakeDeathDraw.BackgroundRow2] = 0x9505,
        [MotherBrainFakeDeathDraw.BackgroundRow3] = 0x9523,
        [MotherBrainFakeDeathDraw.BackgroundRow4] = 0x9541,
        [MotherBrainFakeDeathDraw.BackgroundRow5] = 0x955f,
        [MotherBrainFakeDeathDraw.BackgroundRow6] = 0x957d,
        [MotherBrainFakeDeathDraw.BackgroundRow7] = 0x959b,
        [MotherBrainFakeDeathDraw.BackgroundRow8] = 0x95b9,
        [MotherBrainFakeDeathDraw.BackgroundRow9] = 0x95d7,
        [MotherBrainFakeDeathDraw.BackgroundRowA] = 0x95f5,
        [MotherBrainFakeDeathDraw.BackgroundRowB] = 0x9613,
        [MotherBrainFakeDeathDraw.BackgroundRowC] = 0x9631,
        [MotherBrainFakeDeathDraw.BackgroundRowD] = 0x964f,
        [MotherBrainFakeDeathDraw.BackgroundRowEUnused] = 0x966d,
        [MotherBrainFakeDeathDraw.BackgroundRowFUnused] = 0x968b,
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = 0x96a9,
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = 0x96b1,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = 0x96bf,
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = 0x96cb,
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = 0x96ef,
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = 0x9703,
    };

    /// <summary>The address of each <see cref="MotherBrainGlassDraw"/>.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassDraw, ushort> MotherBrainGlass { get; } = new Dictionary<MotherBrainGlassDraw, ushort>
    {
        [MotherBrainGlassDraw.Initial] = 0x9717,
        [MotherBrainGlassDraw.PaneDamage1] = 0x971d,
        [MotherBrainGlassDraw.PaneDamage2] = 0x9731,
        [MotherBrainGlassDraw.PaneTransition] = 0x9745,
        [MotherBrainGlassDraw.ShiftedPane1] = 0x974f,
        [MotherBrainGlassDraw.ShiftedPane2] = 0x9769,
        [MotherBrainGlassDraw.ShiftedPane3] = 0x9781,
        [MotherBrainGlassDraw.Shatter1] = 0x978f,
        [MotherBrainGlassDraw.Shatter2] = 0x97b7,
        [MotherBrainGlassDraw.Shatter3] = 0x97e7,
        [MotherBrainGlassDraw.Cleared] = 0x9817,
    };

    /// <summary>The address of each <see cref="NoobTubeDraw"/>.</summary>
    internal static IReadOnlyDictionary<NoobTubeDraw, ushort> NoobTube { get; } = new Dictionary<NoobTubeDraw, ushort>
    {
        [NoobTubeDraw.Intact] = 0x98d1,
        [NoobTubeDraw.Damaged] = 0x98d7,
        [NoobTubeDraw.Opened] = 0x98dd,
        [NoobTubeDraw.Cleared] = 0x98e3,
        [NoobTubeDraw.BrokenLate] = 0x9953,
        [NoobTubeDraw.OpenedRows] = 0x9991,
        [NoobTubeDraw.BrokenFull] = 0x99e5,
    };

    /// <summary>The address of each <see cref="RoomPlmBombedRevealDraw"/>.</summary>
    internal static IReadOnlyDictionary<RoomPlmBombedRevealDraw, ushort> RoomPlmBombedReveal { get; } = new Dictionary<RoomPlmBombedRevealDraw, ushort>
    {
        [RoomPlmBombedRevealDraw.CrumbleSingle] = 0xa49b,
        [RoomPlmBombedRevealDraw.PowerBomb] = 0xa4e7,
        [RoomPlmBombedRevealDraw.SuperMissile] = 0xa4ed,
    };

    /// <summary>The address of each <see cref="SamusEaterDraw"/>.</summary>
    internal static IReadOnlyDictionary<SamusEaterDraw, ushort> SamusEater { get; } = new Dictionary<SamusEaterDraw, ushort>
    {
        [SamusEaterDraw.FloorIdle] = 0x9e0d,
        [SamusEaterDraw.FloorChew1] = 0x9e45,
        [SamusEaterDraw.FloorChew2] = 0x9e61,
        [SamusEaterDraw.FloorChew3] = 0x9e7d,
        [SamusEaterDraw.CeilingIdle] = 0x9e99,
        [SamusEaterDraw.CeilingChew1] = 0x9ed1,
        [SamusEaterDraw.CeilingChew2] = 0x9eed,
        [SamusEaterDraw.CeilingChew3] = 0x9f09,
    };

    /// <summary>The address of each <see cref="SporeSpawnCeilingDraw"/>.</summary>
    internal static IReadOnlyDictionary<SporeSpawnCeilingDraw, ushort> SporeSpawnCeiling { get; } = new Dictionary<SporeSpawnCeilingDraw, ushort>
    {
        [SporeSpawnCeilingDraw.Clear] = 0x9413,
        [SporeSpawnCeilingDraw.CrumbleFirst] = 0x9423,
        [SporeSpawnCeilingDraw.CrumbleSecond] = 0x9433,
        [SporeSpawnCeilingDraw.CrumbleThird] = 0x9443,
    };

    /// <summary>The address of each <see cref="StationDraw"/>.</summary>
    internal static IReadOnlyDictionary<StationDraw, ushort> Station { get; } = new Dictionary<StationDraw, ushort>
    {
        [StationDraw.MapFrame0] = 0x9f25,
        [StationDraw.MapFrame1] = 0x9f31,
        [StationDraw.MapFrame2] = 0x9f3d,
        [StationDraw.MapRightRetracted] = 0x9f49,
        [StationDraw.MapRightExtended] = 0x9f55,
        [StationDraw.MapLeftRetracted] = 0x9f5b,
        [StationDraw.MapLeftExtended] = 0x9f67,
        [StationDraw.EnergyFrame0] = 0x9f6d,
        [StationDraw.EnergyFrame1] = 0x9f79,
        [StationDraw.EnergyFrame2] = 0x9f85,
        [StationDraw.MissileFrame0] = 0x9f91,
        [StationDraw.MissileFrame1] = 0x9f9d,
        [StationDraw.MissileFrame2] = 0x9fa9,
        [StationDraw.ResourceRightRetracted] = 0x9fb5,
        [StationDraw.ResourceRightExtended] = 0x9fbb,
        [StationDraw.ResourceLeftRetracted] = 0x9fc1,
        [StationDraw.ResourceLeftExtended] = 0x9fc7,
        [StationDraw.SaveIdle] = 0x9a3f,
        [StationDraw.SaveActive] = 0x9a9f,
        [StationDraw.SaveAlternate] = 0x9a6f,
    };

    /// <summary>The address of each <see cref="TourianAccessDraw"/>.</summary>
    internal static IReadOnlyDictionary<TourianAccessDraw, ushort> TourianAccess { get; } = new Dictionary<TourianAccessDraw, ushort>
    {
        [TourianAccessDraw.EmptyRow] = 0x9297,
        [TourianAccessDraw.CrumbleFirst] = 0x92a3,
        [TourianAccessDraw.CrumbleSecond] = 0x92af,
        [TourianAccessDraw.CrumbleThird] = 0x92bb,
        [TourianAccessDraw.Clear] = 0x92c7,
    };

    /// <summary>The single-block shot-block breakup frames, six bytes apart.</summary>
    internal const ushort SingleBreakupFirst = 0xa345;

    /// <summary>The horizontal-pair breakup frames, eight bytes apart.</summary>
    internal const ushort HorizontalBreakupFirst = 0xa35d;

    /// <summary>The vertical-pair breakup frames, eight bytes apart.</summary>
    internal const ushort VerticalBreakupFirst = 0xa37d;

    /// <summary>The square breakup frames, sixteen bytes apart.</summary>
    internal const ushort SquareBreakupFirst = 0xa39d;

    /// <summary>The horizontal shot-block restoration.</summary>
    internal const ushort ShotBlockRestoreHorizontal = 0xa47b;

    /// <summary>The vertical shot-block restoration.</summary>
    internal const ushort ShotBlockRestoreVertical = 0xa483;

    /// <summary>The square shot-block restoration.</summary>
    internal const ushort ShotBlockRestoreSquare = 0xa48b;

    /// <summary>The five Grapple-block draws, six bytes apart.</summary>
    internal const ushort GrappleBlockFirst = 0xa4f9;

    /// <summary>The empty-item draw.</summary>
    internal const ushort CollectibleEmpty = 0xa2b5;

    /// <summary>The Chozo orb frames, six bytes apart.</summary>
    internal const ushort CollectibleOrbFirst = 0xa2c7;

    /// <summary>The Chozo orb burst.</summary>
    internal const ushort CollectibleOrbBurst = 0xa2d9;

    /// <summary>The tank frames, six bytes apart.</summary>
    internal const ushort CollectibleTankFirst = 0xa2df;

    /// <summary>The dynamically assigned item frames, six bytes apart.</summary>
    internal const ushort CollectibleDynamicFirst = 0xa30f;

    /// <summary>The shot-block reveal frames, six bytes apart.</summary>
    internal const ushort CollectibleShotRevealFirst = 0xa3dd;

    /// <summary>The dynamic-item first-frame selector table.</summary>
    internal const ushort CollectibleDynamicFrame0Table = 0xe05f;

    /// <summary>The dynamic-item second-frame selector table.</summary>
    internal const ushort CollectibleDynamicFrame1Table = 0xe077;

    /// <summary>The left-facing cleared door opening; the other facings follow twelve bytes apart.</summary>
    internal const ushort DoorClearLeft = 0xa677;

    /// <summary>The left-facing grey cap; frames twelve bytes apart, facings 48 bytes apart.</summary>
    internal const ushort GreyDoorLeft = 0xa6a7;

    /// <summary>The left-facing yellow cap; colours 192 bytes apart.</summary>
    internal const ushort ColoredDoorYellowLeft = 0xa767;

    /// <summary>The left-facing blue-cap alias; each facing's alias and four frames span sixty bytes.</summary>
    internal const ushort BlueDoorLeftAlias = 0xa9a7;

    /// <summary>The left eye door's eye frames, eight bytes apart.</summary>
    internal const ushort EyeDoorLeftEyeFirst = 0x9c03;

    /// <summary>The left eye door's middle frames, six bytes apart.</summary>
    internal const ushort EyeDoorLeftMiddleFirst = 0x9c2b;

    /// <summary>The left eye door's bottom frames, six bytes apart.</summary>
    internal const ushort EyeDoorLeftBottomFirst = 0x9c3d;

    /// <summary>The left eye door's opening clear.</summary>
    internal const ushort EyeDoorMirroredOpeningClear = 0x9bf7;

    /// <summary>How far the right eye door's draws follow the left's.</summary>
    internal const ushort EyeDoorRightOffset = 0x0058;

    /// <summary>The Crateria escape passage's clear.</summary>
    internal const ushort CrateriaPassageClearPair = 0x9253;

    /// <summary>The Old Tourian escape shaft wall's opening.</summary>
    internal const ushort OldTourianShaftWallOpen = 0x9283;

    /// <summary>The Maridia elevatube block, shared with Kraid's first ceiling crumble.</summary>
    internal const ushort MaridiaElevatubeBlock = 0x9367;

    /// <summary>The Speed Booster block's bomb reveal.</summary>
    internal const ushort SpeedBoosterBombReveal = 0xa4f3;
}
