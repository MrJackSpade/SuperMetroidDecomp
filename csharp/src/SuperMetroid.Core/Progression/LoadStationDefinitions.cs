namespace SuperMetroid.Core.Progression;

/// <summary>
/// The load stations: the save stations, the elevators' arrivals, the Ceres start and the arrival on Zebes. A save
/// resumes at one, and the used-station record keeps the ones Samus has used.
/// </summary>
public static class LoadStationDefinitions
{
    /// <summary>The gunship on the Landing Site, where a new game on Zebes first saves.</summary>
    public static LoadStation Gunship { get; } = new(nameof(Gunship), AreaId.Crateria);

    /// <summary>The Crateria save room's save station.</summary>
    public static LoadStation CrateriaSave { get; } = new(nameof(CrateriaSave), AreaId.Crateria);

    /// <summary>The Forgotten Highway elevator's arrival in Crateria.</summary>
    public static LoadStation ForgottenHighwayElevator { get; } = new(nameof(ForgottenHighwayElevator), AreaId.Crateria);

    /// <summary>The Red Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation RedBrinstarElevator { get; } = new(nameof(RedBrinstarElevator), AreaId.Crateria);

    /// <summary>The Blue Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation BlueBrinstarElevator { get; } = new(nameof(BlueBrinstarElevator), AreaId.Crateria);

    /// <summary>The Green Brinstar elevator's arrival in Crateria.</summary>
    public static LoadStation GreenBrinstarElevator { get; } = new(nameof(GreenBrinstarElevator), AreaId.Crateria);

    /// <summary>The Tourian elevator's arrival in the Statues room.</summary>
    public static LoadStation StatuesElevator { get; } = new(nameof(StatuesElevator), AreaId.Crateria);

    /// <summary>High above the Landing Site, where the gunship arrives after the Ceres escape.</summary>
    public static LoadStation ZebesArrival { get; } = new(nameof(ZebesArrival), AreaId.Crateria);

    /// <summary>The Big Pink save room's save station.</summary>
    public static LoadStation BigPinkSave { get; } = new(nameof(BigPinkSave), AreaId.Brinstar);

    /// <summary>The Green Brinstar save room's save station.</summary>
    public static LoadStation GreenBrinstarSave { get; } = new(nameof(GreenBrinstarSave), AreaId.Brinstar);

    /// <summary>The Etecoon save room's save station.</summary>
    public static LoadStation EtecoonSave { get; } = new(nameof(EtecoonSave), AreaId.Brinstar);

    /// <summary>The Warehouse save room's save station.</summary>
    public static LoadStation WarehouseSave { get; } = new(nameof(WarehouseSave), AreaId.Brinstar);

    /// <summary>The Red Brinstar save room's save station.</summary>
    public static LoadStation RedBrinstarSave { get; } = new(nameof(RedBrinstarSave), AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Green Brinstar main shaft.</summary>
    public static LoadStation GreenBrinstarMainShaftElevator { get; } = new(nameof(GreenBrinstarMainShaftElevator), AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Morph Ball room.</summary>
    public static LoadStation MorphBallElevator { get; } = new(nameof(MorphBallElevator), AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Caterpillar room.</summary>
    public static LoadStation CaterpillarElevator { get; } = new(nameof(CaterpillarElevator), AreaId.Brinstar);

    /// <summary>The elevator's arrival in the Warehouse entrance.</summary>
    public static LoadStation WarehouseEntranceElevator { get; } = new(nameof(WarehouseEntranceElevator), AreaId.Brinstar);

    /// <summary>The save station beyond Crocomire.</summary>
    public static LoadStation PostCrocomireSave { get; } = new(nameof(PostCrocomireSave), AreaId.Norfair);

    /// <summary>The Bubble Mountain save room's save station.</summary>
    public static LoadStation BubbleMountainSave { get; } = new(nameof(BubbleMountainSave), AreaId.Norfair);

    /// <summary>The Frog save room's save station.</summary>
    public static LoadStation FrogSave { get; } = new(nameof(FrogSave), AreaId.Norfair);

    /// <summary>The save station before Crocomire.</summary>
    public static LoadStation CrocomireSave { get; } = new(nameof(CrocomireSave), AreaId.Norfair);

    /// <summary>The save station by the Lower Norfair elevator.</summary>
    public static LoadStation LowerNorfairElevatorSave { get; } = new(nameof(LowerNorfairElevatorSave), AreaId.Norfair);

    /// <summary>The Lower Norfair save room's save station.</summary>
    public static LoadStation LowerNorfairSave { get; } = new(nameof(LowerNorfairSave), AreaId.Norfair);

    /// <summary>The elevator's arrival in the Business Center.</summary>
    public static LoadStation BusinessCenterElevator { get; } = new(nameof(BusinessCenterElevator), AreaId.Norfair);

    /// <summary>The Lower Norfair elevator's arrival.</summary>
    public static LoadStation LowerNorfairElevator { get; } = new(nameof(LowerNorfairElevator), AreaId.Norfair);

    /// <summary>The elevator's arrival in the Main Hall.</summary>
    public static LoadStation MainHallElevator { get; } = new(nameof(MainHallElevator), AreaId.Norfair);

    /// <summary>The Wrecked Ship save room's save station.</summary>
    public static LoadStation WreckedShipSave { get; } = new(nameof(WreckedShipSave), AreaId.WreckedShip);

    /// <summary>The save station by the Glass Tunnel.</summary>
    public static LoadStation GlassTunnelSave { get; } = new(nameof(GlassTunnelSave), AreaId.Maridia);

    /// <summary>The save station by the Forgotten Highway elevator.</summary>
    public static LoadStation ForgottenHighwaySave { get; } = new(nameof(ForgottenHighwaySave), AreaId.Maridia);

    /// <summary>The Aqueduct save room's save station.</summary>
    public static LoadStation AqueductSave { get; } = new(nameof(AqueductSave), AreaId.Maridia);

    /// <summary>The save station before Draygon.</summary>
    public static LoadStation DraygonSave { get; } = new(nameof(DraygonSave), AreaId.Maridia);

    /// <summary>The elevator's arrival in Maridia.</summary>
    public static LoadStation MaridiaElevator { get; } = new(nameof(MaridiaElevator), AreaId.Maridia);

    /// <summary>The save station before Mother Brain.</summary>
    public static LoadStation MotherBrainSave { get; } = new(nameof(MotherBrainSave), AreaId.Tourian);

    /// <summary>The Upper Tourian save room's save station.</summary>
    public static LoadStation UpperTourianSave { get; } = new(nameof(UpperTourianSave), AreaId.Tourian);

    /// <summary>The elevator's arrival in Tourian's first room.</summary>
    public static LoadStation TourianFirstElevator { get; } = new(nameof(TourianFirstElevator), AreaId.Tourian);

    /// <summary>The Ceres elevator room, where a new game begins.</summary>
    public static LoadStation CeresElevator { get; } = new(nameof(CeresElevator), AreaId.Ceres);

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
    public static IReadOnlyList<LoadStation> Usable { get; } =
    [
        Gunship, CrateriaSave, ForgottenHighwayElevator, RedBrinstarElevator, BlueBrinstarElevator, GreenBrinstarElevator,
        StatuesElevator, BigPinkSave, GreenBrinstarSave, EtecoonSave, WarehouseSave,
        RedBrinstarSave, GreenBrinstarMainShaftElevator, MorphBallElevator, CaterpillarElevator, WarehouseEntranceElevator, PostCrocomireSave,
        BubbleMountainSave, FrogSave, CrocomireSave, LowerNorfairElevatorSave, LowerNorfairSave, BusinessCenterElevator,
        LowerNorfairElevator, MainHallElevator, WreckedShipSave, GlassTunnelSave, ForgottenHighwaySave, AqueductSave,
        DraygonSave, MaridiaElevator, MotherBrainSave, UpperTourianSave, TourianFirstElevator, CeresElevator,
    ];
}
