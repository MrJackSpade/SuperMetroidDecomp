using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The load stations: the save stations, the elevators' arrivals, the Ceres start and the arrival on Zebes, each with
/// the room, door and placement loading at it uses.
/// </summary>
public static class LoadStationDefinitions
{
    /// <summary>The gunship on the Landing Site, where a new game on Zebes first saves.</summary>
    public static LoadStation Gunship { get; } = new(nameof(Gunship), AreaId.Crateria, 0, RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(1024, 1024), new(0, 64));

    /// <summary>The Crateria save room's save station.</summary>
    public static LoadStation CrateriaSave { get; } = new(nameof(CrateriaSave), AreaId.Crateria, 1, RoomHeaderDefinitions.CrateriaSave, CrateriaDoorDefinitions.Parlor5, new(0, 0), new(-32, 152));

    /// <summary>The Forgotten Highway elevator's arrival in Crateria.</summary>
    public static LoadStation ForgottenHighwayElevator { get; } = new(nameof(ForgottenHighwayElevator), AreaId.Crateria, 8, RoomHeaderDefinitions.ForgottenHighwayElev, CrateriaDoorDefinitions.ForgottenHighwayElbow1, new(0, 0), new(0, 168));

    /// <summary>The Red Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation RedBrinstarElevator { get; } = new(nameof(RedBrinstarElevator), AreaId.Crateria, 9, RoomHeaderDefinitions.RedBinstarElev, CrateriaDoorDefinitions.CrateriaKihunter2, new(0, 0), new(0, 168));

    /// <summary>The Blue Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation BlueBrinstarElevator { get; } = new(nameof(BlueBrinstarElevator), AreaId.Crateria, 10, RoomHeaderDefinitions.ElevToBlueBrinstar, CrateriaDoorDefinitions.Pit1, new(0, 0), new(0, 136));

    /// <summary>The Green Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation GreenBrinstarElevator { get; } = new(nameof(GreenBrinstarElevator), AreaId.Crateria, 11, RoomHeaderDefinitions.GreenBrinstarElev, CrateriaDoorDefinitions.LowerMushrooms1, new(0, 0), new(0, 136));

    /// <summary>The Tourian elevator's arrival in the Statues room.</summary>
    public static LoadStation StatuesElevator { get; } = new(nameof(StatuesElevator), AreaId.Crateria, 12, RoomHeaderDefinitions.Statues, CrateriaDoorDefinitions.StatuesHallway1, new(0, 256), new(0, 152));

    /// <summary>High above the Landing Site, where the gunship arrives after the Ceres escape.</summary>
    public static LoadStation ZebesArrival { get; } = new(nameof(ZebesArrival), AreaId.Crateria, 18, RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(1024, 0), new(0, 128));

    /// <summary>The Big Pink save room's save station.</summary>
    public static LoadStation BigPinkSave { get; } = new(nameof(BigPinkSave), AreaId.Brinstar, 0, RoomHeaderDefinitions.BigPinkSaveRoom, BrinstarDoorDefinitions.BigPink4, new(0, 0), new(-32, 152));

    /// <summary>The Green Brinstar save room's save station.</summary>
    public static LoadStation GreenBrinstarSave { get; } = new(nameof(GreenBrinstarSave), AreaId.Brinstar, 1, RoomHeaderDefinitions.GreenBrinstarSave, BrinstarDoorDefinitions.GreenBrinstarMainShaft10, new(0, 0), new(-32, 152));

    /// <summary>The Etecoon save room's save station.</summary>
    public static LoadStation EtecoonSave { get; } = new(nameof(EtecoonSave), AreaId.Brinstar, 2, RoomHeaderDefinitions.EtecoonSave, BrinstarDoorDefinitions.EtecoonETank3, new(0, 0), new(-32, 152));

    /// <summary>The Warehouse save room's save station.</summary>
    public static LoadStation WarehouseSave { get; } = new(nameof(WarehouseSave), AreaId.Brinstar, 3, RoomHeaderDefinitions.WarehouseSave, BrinstarDoorDefinitions.WarehouseKihunter2, new(0, 0), new(0, 152));

    /// <summary>The Red Brinstar save room's save station.</summary>
    public static LoadStation RedBrinstarSave { get; } = new(nameof(RedBrinstarSave), AreaId.Brinstar, 4, RoomHeaderDefinitions.RedBrinstarSave, BrinstarDoorDefinitions.Caterpillar6, new(0, 0), new(0, 152));

    /// <summary>The elevator's arrival in the Green Brinstar main shaft.</summary>
    public static LoadStation GreenBrinstarMainShaftElevator { get; } = new(nameof(GreenBrinstarMainShaftElevator), AreaId.Brinstar, 8, RoomHeaderDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMap1, new(0, 512), new(0, 168));

    /// <summary>The elevator's arrival in the Morph Ball room.</summary>
    public static LoadStation MorphBallElevator { get; } = new(nameof(MorphBallElevator), AreaId.Brinstar, 9, RoomHeaderDefinitions.MorphBall, BrinstarDoorDefinitions.GreenHillZone1, new(1280, 512), new(0, 168));

    /// <summary>The elevator's arrival in the Caterpillar room.</summary>
    public static LoadStation CaterpillarElevator { get; } = new(nameof(CaterpillarElevator), AreaId.Brinstar, 10, RoomHeaderDefinitions.Caterpillar, BrinstarDoorDefinitions.Hellway1, new(0, 512), new(0, 168));

    /// <summary>The elevator's arrival in the Warehouse entrance.</summary>
    public static LoadStation WarehouseEntranceElevator { get; } = new(nameof(WarehouseEntranceElevator), AreaId.Brinstar, 11, RoomHeaderDefinitions.WarehouseEntrance, MaridiaDoorDefinitions.EastTunnel1, new(0, 0), new(0, 136));

    /// <summary>The save station beyond Crocomire.</summary>
    public static LoadStation PostCrocomireSave { get; } = new(nameof(PostCrocomireSave), AreaId.Norfair, 0, RoomHeaderDefinitions.PostCrocSave, NorfairDoorDefinitions.PostCrocFarming3, new(0, 0), new(0, 152));

    /// <summary>The Bubble Mountain save room's save station.</summary>
    public static LoadStation BubbleMountainSave { get; } = new(nameof(BubbleMountainSave), AreaId.Norfair, 1, RoomHeaderDefinitions.BubbleMountainSave, NorfairDoorDefinitions.BubbleMountain6, new(0, 0), new(-32, 152));

    /// <summary>The Frog save room's save station.</summary>
    public static LoadStation FrogSave { get; } = new(nameof(FrogSave), AreaId.Norfair, 2, RoomHeaderDefinitions.FrogSave, NorfairDoorDefinitions.FrogSpeedway0, new(0, 0), new(0, 152));

    /// <summary>The save station before Crocomire.</summary>
    public static LoadStation CrocomireSave { get; } = new(nameof(CrocomireSave), AreaId.Norfair, 3, RoomHeaderDefinitions.CrocomireSave, NorfairDoorDefinitions.CrocomireSpeedway2, new(0, 0), new(0, 152));

    /// <summary>The save station by the Lower Norfair elevator.</summary>
    public static LoadStation LowerNorfairElevatorSave { get; } = new(nameof(LowerNorfairElevatorSave), AreaId.Norfair, 4, RoomHeaderDefinitions.LNElevSave, NorfairDoorDefinitions.LowerNorfairElev3, new(0, 0), new(-32, 152));

    /// <summary>The Lower Norfair save room's save station.</summary>
    public static LoadStation LowerNorfairSave { get; } = new(nameof(LowerNorfairSave), AreaId.Norfair, 5, RoomHeaderDefinitions.LNSave, NorfairDoorDefinitions.RedKihunterShaft3, new(0, 0), new(0, 152));

    /// <summary>The elevator's arrival in the Business Center.</summary>
    public static LoadStation BusinessCenterElevator { get; } = new(nameof(BusinessCenterElevator), AreaId.Norfair, 8, RoomHeaderDefinitions.BusinessCenter, NorfairDoorDefinitions.CathedralEntrance0, new(0, 512), new(0, 168));

    /// <summary>The Lower Norfair elevator's arrival.</summary>
    public static LoadStation LowerNorfairElevator { get; } = new(nameof(LowerNorfairElevator), AreaId.Norfair, 9, RoomHeaderDefinitions.LowerNorfairElev, NorfairDoorDefinitions.LavaDive1, new(0, 0), new(0, 136));

    /// <summary>The elevator's arrival in the Main Hall.</summary>
    public static LoadStation MainHallElevator { get; } = new(nameof(MainHallElevator), AreaId.Norfair, 10, RoomHeaderDefinitions.MainHall, NorfairDoorDefinitions.AcidStatue1, new(1024, 512), new(0, 136));

    /// <summary>The Wrecked Ship save room's save station.</summary>
    public static LoadStation WreckedShipSave { get; } = new(nameof(WreckedShipSave), AreaId.WreckedShip, 0, RoomHeaderDefinitions.WreckedShipSave, WreckedShipDoorDefinitions.WreckedShipMainShaft6, new(0, 0), new(0, 152));

    /// <summary>The save station by the Glass Tunnel.</summary>
    public static LoadStation GlassTunnelSave { get; } = new(nameof(GlassTunnelSave), AreaId.Maridia, 0, RoomHeaderDefinitions.GlassTunnelSave, MaridiaDoorDefinitions.GlassTunnel3, new(0, 0), new(0, 152));

    /// <summary>The save station by the Forgotten Highway elevator.</summary>
    public static LoadStation ForgottenHighwaySave { get; } = new(nameof(ForgottenHighwaySave), AreaId.Maridia, 1, RoomHeaderDefinitions.ForgottenHighwaySave, MaridiaDoorDefinitions.MaridiaElev1, new(0, 0), new(0, 152));

    /// <summary>The Aqueduct save room's save station.</summary>
    public static LoadStation AqueductSave { get; } = new(nameof(AqueductSave), AreaId.Maridia, 2, RoomHeaderDefinitions.AqueductSave, MaridiaDoorDefinitions.Aqueduct5, new(0, 0), new(-32, 152));

    /// <summary>The save station before Draygon.</summary>
    public static LoadStation DraygonSave { get; } = new(nameof(DraygonSave), AreaId.Maridia, 3, RoomHeaderDefinitions.DraygonSave, MaridiaDoorDefinitions.Colosseum1, new(0, 0), new(0, 152));

    /// <summary>The elevator's arrival in Maridia.</summary>
    public static LoadStation MaridiaElevator { get; } = new(nameof(MaridiaElevator), AreaId.Maridia, 8, RoomHeaderDefinitions.MaridiaElev, MaridiaDoorDefinitions.ThreadTheNeedle1, new(0, 512), new(0, 168));

    /// <summary>The save station before Mother Brain.</summary>
    public static LoadStation MotherBrainSave { get; } = new(nameof(MotherBrainSave), AreaId.Tourian, 0, RoomHeaderDefinitions.MotherBrainSave, TourianDoorDefinitions.RinkaShaft1, new(0, 0), new(-32, 152));

    /// <summary>The Upper Tourian save room's save station.</summary>
    public static LoadStation UpperTourianSave { get; } = new(nameof(UpperTourianSave), AreaId.Tourian, 1, RoomHeaderDefinitions.UpperTourianSave, TourianDoorDefinitions.TourianFirst3, new(0, 0), new(0, 152));

    /// <summary>The elevator's arrival in Tourian's first room.</summary>
    public static LoadStation TourianFirstElevator { get; } = new(nameof(TourianFirstElevator), AreaId.Tourian, 8, RoomHeaderDefinitions.TourianFirst, TourianDoorDefinitions.Metroids10, new(0, 512), new(0, 168));

    /// <summary>The Ceres elevator room, where a new game begins.</summary>
    public static LoadStation CeresElevator { get; } = new(nameof(CeresElevator), AreaId.Ceres, 0, RoomHeaderDefinitions.CeresElev, CeresDoorDefinitions.FallingTile0, new(0, 0), new(0, 72));

    /// <summary>Every load station.</summary>
    internal static IReadOnlyList<LoadStation> All { get; } =
    [
        Gunship, CrateriaSave, ForgottenHighwayElevator, RedBrinstarElevator, BlueBrinstarElevator, GreenBrinstarElevator,
        StatuesElevator, ZebesArrival, BigPinkSave, GreenBrinstarSave, EtecoonSave, WarehouseSave,
        RedBrinstarSave, GreenBrinstarMainShaftElevator, MorphBallElevator, CaterpillarElevator, WarehouseEntranceElevator, PostCrocomireSave,
        BubbleMountainSave, FrogSave, CrocomireSave, LowerNorfairElevatorSave, LowerNorfairSave, BusinessCenterElevator,
        LowerNorfairElevator, MainHallElevator, WreckedShipSave, GlassTunnelSave, ForgottenHighwaySave, AqueductSave,
        DraygonSave, MaridiaElevator, MotherBrainSave, UpperTourianSave, TourianFirstElevator, CeresElevator,
    ];
}
