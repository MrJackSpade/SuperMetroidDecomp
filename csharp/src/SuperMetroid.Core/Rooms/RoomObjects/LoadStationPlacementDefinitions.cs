using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where loading at each load station puts Samus: the room, the door it is entered through, the initial camera and
/// Samus's placement relative to the camera.
/// </summary>
internal static class LoadStationPlacementDefinitions
{
    /// <summary>Each load station's placement.</summary>
    internal static IReadOnlyDictionary<LoadStation, LoadStationPlacement> ByStation { get; } = new Dictionary<LoadStation, LoadStationPlacement>
    {
        [LoadStationDefinitions.Gunship] = new(RoomDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(1024, 1024), new(0, 64)),
        [LoadStationDefinitions.CrateriaSave] = new(RoomDefinitions.CrateriaSave, CrateriaDoorDefinitions.Parlor5, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.ForgottenHighwayElevator] = new(RoomDefinitions.ForgottenHighwayElev, CrateriaDoorDefinitions.ForgottenHighwayElbow1, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.RedBrinstarElevator] = new(RoomDefinitions.RedBinstarElev, CrateriaDoorDefinitions.CrateriaKihunter2, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.BlueBrinstarElevator] = new(RoomDefinitions.ElevToBlueBrinstar, CrateriaDoorDefinitions.Pit1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.GreenBrinstarElevator] = new(RoomDefinitions.GreenBrinstarElev, CrateriaDoorDefinitions.LowerMushrooms1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.StatuesElevator] = new(RoomDefinitions.Statues, CrateriaDoorDefinitions.StatuesHallway1, new(0, 256), new(0, 152)),
        [LoadStationDefinitions.ZebesArrival] = new(RoomDefinitions.LandingSite, CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(1024, 0), new(0, 128)),
        [LoadStationDefinitions.BigPinkSave] = new(RoomDefinitions.BigPinkSaveRoom, BrinstarDoorDefinitions.BigPink4, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.GreenBrinstarSave] = new(RoomDefinitions.GreenBrinstarSave, BrinstarDoorDefinitions.GreenBrinstarMainShaft10, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.EtecoonSave] = new(RoomDefinitions.EtecoonSave, BrinstarDoorDefinitions.EtecoonETank3, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.WarehouseSave] = new(RoomDefinitions.WarehouseSave, BrinstarDoorDefinitions.WarehouseKihunter2, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.RedBrinstarSave] = new(RoomDefinitions.RedBrinstarSave, BrinstarDoorDefinitions.Caterpillar6, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GreenBrinstarMainShaftElevator] = new(RoomDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMap1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MorphBallElevator] = new(RoomDefinitions.MorphBall, BrinstarDoorDefinitions.GreenHillZone1, new(1280, 512), new(0, 168)),
        [LoadStationDefinitions.CaterpillarElevator] = new(RoomDefinitions.Caterpillar, BrinstarDoorDefinitions.Hellway1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.WarehouseEntranceElevator] = new(RoomDefinitions.WarehouseEntrance, MaridiaDoorDefinitions.EastTunnel1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.PostCrocomireSave] = new(RoomDefinitions.PostCrocSave, NorfairDoorDefinitions.PostCrocFarming3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BubbleMountainSave] = new(RoomDefinitions.BubbleMountainSave, NorfairDoorDefinitions.BubbleMountain6, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.FrogSave] = new(RoomDefinitions.FrogSave, NorfairDoorDefinitions.FrogSpeedway0, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.CrocomireSave] = new(RoomDefinitions.CrocomireSave, NorfairDoorDefinitions.CrocomireSpeedway2, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.LowerNorfairElevatorSave] = new(RoomDefinitions.LNElevSave, NorfairDoorDefinitions.LowerNorfairElev3, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.LowerNorfairSave] = new(RoomDefinitions.LNSave, NorfairDoorDefinitions.RedKihunterShaft3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BusinessCenterElevator] = new(RoomDefinitions.BusinessCenter, NorfairDoorDefinitions.CathedralEntrance0, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.LowerNorfairElevator] = new(RoomDefinitions.LowerNorfairElev, NorfairDoorDefinitions.LavaDive1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.MainHallElevator] = new(RoomDefinitions.MainHall, NorfairDoorDefinitions.AcidStatue1, new(1024, 512), new(0, 136)),
        [LoadStationDefinitions.WreckedShipSave] = new(RoomDefinitions.WreckedShipSave, WreckedShipDoorDefinitions.WreckedShipMainShaft6, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GlassTunnelSave] = new(RoomDefinitions.GlassTunnelSave, MaridiaDoorDefinitions.GlassTunnel3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.ForgottenHighwaySave] = new(RoomDefinitions.ForgottenHighwaySave, MaridiaDoorDefinitions.MaridiaElev1, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.AqueductSave] = new(RoomDefinitions.AqueductSave, MaridiaDoorDefinitions.Aqueduct5, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.DraygonSave] = new(RoomDefinitions.DraygonSave, MaridiaDoorDefinitions.Colosseum1, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.MaridiaElevator] = new(RoomDefinitions.MaridiaElev, MaridiaDoorDefinitions.ThreadTheNeedle1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MotherBrainSave] = new(RoomDefinitions.MotherBrainSave, TourianDoorDefinitions.RinkaShaft1, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.UpperTourianSave] = new(RoomDefinitions.UpperTourianSave, TourianDoorDefinitions.TourianFirst3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.TourianFirstElevator] = new(RoomDefinitions.TourianFirst, TourianDoorDefinitions.Metroids10, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.CeresElevator] = new(RoomDefinitions.CeresElev, CeresDoorDefinitions.FallingTile0, new(0, 0), new(0, 72)),
    };
}
