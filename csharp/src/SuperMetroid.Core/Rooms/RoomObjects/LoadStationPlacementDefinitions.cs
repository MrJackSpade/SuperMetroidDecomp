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
        [LoadStationDefinitions.Gunship] = new(RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(1024, 1024), new(0, 64)),
        [LoadStationDefinitions.CrateriaSave] = new(RoomHeaderDefinitions.CrateriaSave, CrateriaDoorDefinitions.Parlor5, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.ForgottenHighwayElevator] = new(RoomHeaderDefinitions.ForgottenHighwayElev, CrateriaDoorDefinitions.ForgottenHighwayElbow1, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.RedBrinstarElevator] = new(RoomHeaderDefinitions.RedBinstarElev, CrateriaDoorDefinitions.CrateriaKihunter2, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.BlueBrinstarElevator] = new(RoomHeaderDefinitions.ElevToBlueBrinstar, CrateriaDoorDefinitions.Pit1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.GreenBrinstarElevator] = new(RoomHeaderDefinitions.GreenBrinstarElev, CrateriaDoorDefinitions.LowerMushrooms1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.StatuesElevator] = new(RoomHeaderDefinitions.Statues, CrateriaDoorDefinitions.StatuesHallway1, new(0, 256), new(0, 152)),
        [LoadStationDefinitions.ZebesArrival] = new(RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(1024, 0), new(0, 128)),
        [LoadStationDefinitions.BigPinkSave] = new(RoomHeaderDefinitions.BigPinkSaveRoom, BrinstarDoorDefinitions.BigPink4, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.GreenBrinstarSave] = new(RoomHeaderDefinitions.GreenBrinstarSave, BrinstarDoorDefinitions.GreenBrinstarMainShaft10, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.EtecoonSave] = new(RoomHeaderDefinitions.EtecoonSave, BrinstarDoorDefinitions.EtecoonETank3, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.WarehouseSave] = new(RoomHeaderDefinitions.WarehouseSave, BrinstarDoorDefinitions.WarehouseKihunter2, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.RedBrinstarSave] = new(RoomHeaderDefinitions.RedBrinstarSave, BrinstarDoorDefinitions.Caterpillar6, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GreenBrinstarMainShaftElevator] = new(RoomHeaderDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMap1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MorphBallElevator] = new(RoomHeaderDefinitions.MorphBall, BrinstarDoorDefinitions.GreenHillZone1, new(1280, 512), new(0, 168)),
        [LoadStationDefinitions.CaterpillarElevator] = new(RoomHeaderDefinitions.Caterpillar, BrinstarDoorDefinitions.Hellway1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.WarehouseEntranceElevator] = new(RoomHeaderDefinitions.WarehouseEntrance, MaridiaDoorDefinitions.EastTunnel1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.PostCrocomireSave] = new(RoomHeaderDefinitions.PostCrocSave, NorfairDoorDefinitions.PostCrocFarming3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BubbleMountainSave] = new(RoomHeaderDefinitions.BubbleMountainSave, NorfairDoorDefinitions.BubbleMountain6, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.FrogSave] = new(RoomHeaderDefinitions.FrogSave, NorfairDoorDefinitions.FrogSpeedway0, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.CrocomireSave] = new(RoomHeaderDefinitions.CrocomireSave, NorfairDoorDefinitions.CrocomireSpeedway2, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.LowerNorfairElevatorSave] = new(RoomHeaderDefinitions.LNElevSave, NorfairDoorDefinitions.LowerNorfairElev3, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.LowerNorfairSave] = new(RoomHeaderDefinitions.LNSave, NorfairDoorDefinitions.RedKihunterShaft3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BusinessCenterElevator] = new(RoomHeaderDefinitions.BusinessCenter, NorfairDoorDefinitions.CathedralEntrance0, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.LowerNorfairElevator] = new(RoomHeaderDefinitions.LowerNorfairElev, NorfairDoorDefinitions.LavaDive1, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.MainHallElevator] = new(RoomHeaderDefinitions.MainHall, NorfairDoorDefinitions.AcidStatue1, new(1024, 512), new(0, 136)),
        [LoadStationDefinitions.WreckedShipSave] = new(RoomHeaderDefinitions.WreckedShipSave, WreckedShipDoorDefinitions.WreckedShipMainShaft6, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GlassTunnelSave] = new(RoomHeaderDefinitions.GlassTunnelSave, MaridiaDoorDefinitions.GlassTunnel3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.ForgottenHighwaySave] = new(RoomHeaderDefinitions.ForgottenHighwaySave, MaridiaDoorDefinitions.MaridiaElev1, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.AqueductSave] = new(RoomHeaderDefinitions.AqueductSave, MaridiaDoorDefinitions.Aqueduct5, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.DraygonSave] = new(RoomHeaderDefinitions.DraygonSave, MaridiaDoorDefinitions.Colosseum1, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.MaridiaElevator] = new(RoomHeaderDefinitions.MaridiaElev, MaridiaDoorDefinitions.ThreadTheNeedle1, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MotherBrainSave] = new(RoomHeaderDefinitions.MotherBrainSave, TourianDoorDefinitions.RinkaShaft1, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.UpperTourianSave] = new(RoomHeaderDefinitions.UpperTourianSave, TourianDoorDefinitions.TourianFirst3, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.TourianFirstElevator] = new(RoomHeaderDefinitions.TourianFirst, TourianDoorDefinitions.Metroids10, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.CeresElevator] = new(RoomHeaderDefinitions.CeresElev, CeresDoorDefinitions.FallingTile0, new(0, 0), new(0, 72)),
    };
}
