namespace SuperMetroid.Core.Progression;

/// <summary>
/// The load stations: the save stations, the elevators' arrivals, the Ceres start and the arrival on Zebes. A save
/// resumes at one, and the used-station record keeps the ones Samus has used.
/// </summary>
public static class LoadStationDefinitions
{
    /// <summary>The gunship on the Landing Site, where a new game on Zebes first saves.</summary>
    public static LoadStation Gunship { get; } = new("Gunship", AreaId.Crateria);

    /// <summary>The Crateria save room's save station.</summary>
    public static LoadStation CrateriaSave { get; } = new("CrateriaSave", AreaId.Crateria);

    /// <summary>The Forgotten Highway elevator's arrival in Crateria.</summary>
    public static LoadStation ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator", AreaId.Crateria);

    /// <summary>The Red Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation RedBrinstarElevator { get; } = new("RedBrinstarElevator", AreaId.Crateria);

    /// <summary>The Blue Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation BlueBrinstarElevator { get; } = new("BlueBrinstarElevator", AreaId.Crateria);

    /// <summary>The Green Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation GreenBrinstarElevator { get; } = new("GreenBrinstarElevator", AreaId.Crateria);

    /// <summary>The Tourian elevator's arrival in the Statues room.</summary>
    public static LoadStation StatuesElevator { get; } = new("StatuesElevator", AreaId.Crateria);

    /// <summary>High above the Landing Site, where the gunship arrives after the Ceres escape.</summary>
    public static LoadStation ZebesArrival { get; } = new("ZebesArrival", AreaId.Crateria);

    /// <summary>The Big Pink save room's save station.</summary>
    public static LoadStation BigPinkSave { get; } = new("BigPinkSave", AreaId.Brinstar);

    /// <summary>The Green Brinstar save room's save station.</summary>
    public static LoadStation GreenBrinstarSave { get; } = new("GreenBrinstarSave", AreaId.Brinstar);

    /// <summary>The Etecoon save room's save station.</summary>
    public static LoadStation EtecoonSave { get; } = new("EtecoonSave", AreaId.Brinstar);

    /// <summary>The Warehouse save room's save station.</summary>
    public static LoadStation WarehouseSave { get; } = new("WarehouseSave", AreaId.Brinstar);

    /// <summary>The Red Brinstar save room's save station.</summary>
    public static LoadStation RedBrinstarSave { get; } = new("RedBrinstarSave", AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Green Brinstar main shaft.</summary>
    public static LoadStation GreenBrinstarMainShaftElevator { get; } = new("GreenBrinstarMainShaftElevator", AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Morph Ball room.</summary>
    public static LoadStation MorphBallElevator { get; } = new("MorphBallElevator", AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Caterpillar room.</summary>
    public static LoadStation CaterpillarElevator { get; } = new("CaterpillarElevator", AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Warehouse entrance.</summary>
    public static LoadStation WarehouseEntranceElevator { get; } = new("WarehouseEntranceElevator", AreaId.Brinstar);

    /// <summary>The save station beyond Crocomire.</summary>
    public static LoadStation PostCrocomireSave { get; } = new("PostCrocomireSave", AreaId.Norfair);

    /// <summary>The Bubble Mountain save room's save station.</summary>
    public static LoadStation BubbleMountainSave { get; } = new("BubbleMountainSave", AreaId.Norfair);

    /// <summary>The Frog save room's save station.</summary>
    public static LoadStation FrogSave { get; } = new("FrogSave", AreaId.Norfair);

    /// <summary>The save station before Crocomire.</summary>
    public static LoadStation CrocomireSave { get; } = new("CrocomireSave", AreaId.Norfair);

    /// <summary>The save station by the Lower Norfair elevator.</summary>
    public static LoadStation LowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorSave", AreaId.Norfair);

    /// <summary>The Lower Norfair save room's save station.</summary>
    public static LoadStation LowerNorfairSave { get; } = new("LowerNorfairSave", AreaId.Norfair);

    /// <summary>The elevator's arrival in the Business Center.</summary>
    public static LoadStation BusinessCenterElevator { get; } = new("BusinessCenterElevator", AreaId.Norfair);

    /// <summary>The Lower Norfair elevator's arrival.</summary>
    public static LoadStation LowerNorfairElevator { get; } = new("LowerNorfairElevator", AreaId.Norfair);

    /// <summary>The elevator's arrival in the Main Hall.</summary>
    public static LoadStation MainHallElevator { get; } = new("MainHallElevator", AreaId.Norfair);

    /// <summary>The Wrecked Ship save room's save station.</summary>
    public static LoadStation WreckedShipSave { get; } = new("WreckedShipSave", AreaId.WreckedShip);

    /// <summary>The save station by the Glass Tunnel.</summary>
    public static LoadStation GlassTunnelSave { get; } = new("GlassTunnelSave", AreaId.Maridia);

    /// <summary>The save station by the Forgotten Highway elevator.</summary>
    public static LoadStation ForgottenHighwaySave { get; } = new("ForgottenHighwaySave", AreaId.Maridia);

    /// <summary>The Aqueduct save room's save station.</summary>
    public static LoadStation AqueductSave { get; } = new("AqueductSave", AreaId.Maridia);

    /// <summary>The save station before Draygon.</summary>
    public static LoadStation DraygonSave { get; } = new("DraygonSave", AreaId.Maridia);

    /// <summary>The elevator's arrival in Maridia.</summary>
    public static LoadStation MaridiaElevator { get; } = new("MaridiaElevator", AreaId.Maridia);

    /// <summary>The save station before Mother Brain.</summary>
    public static LoadStation MotherBrainSave { get; } = new("MotherBrainSave", AreaId.Tourian);

    /// <summary>The Upper Tourian save room's save station.</summary>
    public static LoadStation UpperTourianSave { get; } = new("UpperTourianSave", AreaId.Tourian);

    /// <summary>The elevator's arrival in Tourian's first room.</summary>
    public static LoadStation TourianFirstElevator { get; } = new("TourianFirstElevator", AreaId.Tourian);

    /// <summary>The Ceres elevator room, where a new game begins.</summary>
    public static LoadStation CeresElevator { get; } = new("CeresElevator", AreaId.Ceres);

    /// <summary>Every load station.</summary>
    public static IReadOnlyList<LoadStation> All { get; } =
    [
        Gunship, CrateriaSave, ForgottenHighwayElevator, RedBrinstarElevator, BlueBrinstarElevator, GreenBrinstarElevator,
        StatuesElevator, ZebesArrival, BigPinkSave, GreenBrinstarSave, EtecoonSave, WarehouseSave,
        RedBrinstarSave, GreenBrinstarMainShaftElevator, MorphBallElevator, CaterpillarElevator, WarehouseEntranceElevator, PostCrocomireSave,
        BubbleMountainSave, FrogSave, CrocomireSave, LowerNorfairElevatorSave, LowerNorfairSave, BusinessCenterElevator,
        LowerNorfairElevator, MainHallElevator, WreckedShipSave, GlassTunnelSave, ForgottenHighwaySave, AqueductSave,
        DraygonSave, MaridiaElevator, MotherBrainSave, UpperTourianSave, TourianFirstElevator, CeresElevator,
    ];

    /// <summary>
    /// The stations Samus uses, which the used-station record keeps: every station but the arrival on Zebes, where only
    /// the gunship's landing loads her.
    /// </summary>
    public static IReadOnlyList<LoadStation> Usable { get; } = [.. All.Where(station => station != ZebesArrival)];
}
