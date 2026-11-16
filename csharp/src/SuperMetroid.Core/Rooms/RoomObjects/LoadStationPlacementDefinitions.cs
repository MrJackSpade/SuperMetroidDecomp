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
        [LoadStationDefinitions.Gunship] = new(RoomDefinitions.LandingSite, CrateriaDoorDefinitions.ParlorToLandingSite, new(1024, 1024), new(0, 64)),
        [LoadStationDefinitions.CrateriaSave] = new(RoomDefinitions.CrateriaSave, CrateriaDoorDefinitions.ParlorToCrateriaSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.ForgottenHighwayElevator] = new(RoomDefinitions.ForgottenHighwayElevator, CrateriaDoorDefinitions.ForgottenHighwayElbowToForgottenHighwayElevator, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.RedBrinstarElevator] = new(RoomDefinitions.RedBrinstarElevator, CrateriaDoorDefinitions.CrateriaKihunterToRedBrinstarElevator, new(0, 0), new(0, 168)),
        [LoadStationDefinitions.BlueBrinstarElevator] = new(RoomDefinitions.ElevatorToBlueBrinstar, CrateriaDoorDefinitions.PitToElevatorToBlueBrinstar, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.GreenBrinstarElevator] = new(RoomDefinitions.GreenBrinstarElevator, CrateriaDoorDefinitions.LowerMushroomsToGreenBrinstarElevator, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.StatuesElevator] = new(RoomDefinitions.Statues, CrateriaDoorDefinitions.StatuesHallwayToStatues, new(0, 256), new(0, 152)),
        [LoadStationDefinitions.ZebesArrival] = new(RoomDefinitions.LandingSite, CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(1024, 0), new(0, 128)),
        [LoadStationDefinitions.BigPinkSave] = new(RoomDefinitions.BigPinkSaveRoom, BrinstarDoorDefinitions.BigPinkToBigPinkSaveRoom, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.GreenBrinstarSave] = new(RoomDefinitions.GreenBrinstarSave, BrinstarDoorDefinitions.GreenBrinstarMainShaftToGreenBrinstarSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.EtecoonSave] = new(RoomDefinitions.EtecoonSave, BrinstarDoorDefinitions.EtecoonEnergyTankToEtecoonSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.WarehouseSave] = new(RoomDefinitions.WarehouseSave, BrinstarDoorDefinitions.WarehouseKihunterToWarehouseSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.RedBrinstarSave] = new(RoomDefinitions.RedBrinstarSave, BrinstarDoorDefinitions.CaterpillarToRedBrinstarSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GreenBrinstarMainShaftElevator] = new(RoomDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMapToGreenBrinstarMainShaft, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MorphBallElevator] = new(RoomDefinitions.MorphBall, BrinstarDoorDefinitions.GreenHillZoneToMorphBall, new(1280, 512), new(0, 168)),
        [LoadStationDefinitions.CaterpillarElevator] = new(RoomDefinitions.Caterpillar, BrinstarDoorDefinitions.HellwayToCaterpillar, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.WarehouseEntranceElevator] = new(RoomDefinitions.WarehouseEntrance, MaridiaDoorDefinitions.EastTunnelToWarehouseEntrance, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.PostCrocomireSave] = new(RoomDefinitions.PostCrocomireSave, NorfairDoorDefinitions.PostCrocomireFarmingToPostCrocomireSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BubbleMountainSave] = new(RoomDefinitions.BubbleMountainSave, NorfairDoorDefinitions.BubbleMountainToBubbleMountainSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.FrogSave] = new(RoomDefinitions.FrogSave, NorfairDoorDefinitions.FrogSpeedwayToFrogSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.CrocomireSave] = new(RoomDefinitions.CrocomireSave, NorfairDoorDefinitions.CrocomireSpeedwayToCrocomireSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.LowerNorfairElevatorSave] = new(RoomDefinitions.LowerNorfairElevatorSave, NorfairDoorDefinitions.LowerNorfairElevatorToLowerNorfairElevatorSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.LowerNorfairSave] = new(RoomDefinitions.LowerNorfairSave, NorfairDoorDefinitions.RedKihunterShaftToLowerNorfairSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.BusinessCenterElevator] = new(RoomDefinitions.BusinessCenter, NorfairDoorDefinitions.CathedralEntranceToBusinessCenter, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.LowerNorfairElevator] = new(RoomDefinitions.LowerNorfairElevator, NorfairDoorDefinitions.LavaDiveToLowerNorfairElevator, new(0, 0), new(0, 136)),
        [LoadStationDefinitions.MainHallElevator] = new(RoomDefinitions.MainHall, NorfairDoorDefinitions.AcidStatueToMainHall, new(1024, 512), new(0, 136)),
        [LoadStationDefinitions.WreckedShipSave] = new(RoomDefinitions.WreckedShipSave, WreckedShipDoorDefinitions.WreckedShipMainShaftToWreckedShipSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.GlassTunnelSave] = new(RoomDefinitions.GlassTunnelSave, MaridiaDoorDefinitions.GlassTunnelToGlassTunnelSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.ForgottenHighwaySave] = new(RoomDefinitions.ForgottenHighwaySave, MaridiaDoorDefinitions.MaridiaElevatorToForgottenHighwaySave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.AqueductSave] = new(RoomDefinitions.AqueductSave, MaridiaDoorDefinitions.AqueductToAqueductSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.DraygonSave] = new(RoomDefinitions.DraygonSave, MaridiaDoorDefinitions.ColosseumToDraygonSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.MaridiaElevator] = new(RoomDefinitions.MaridiaElevator, MaridiaDoorDefinitions.ThreadTheNeedleToMaridiaElevator, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.MotherBrainSave] = new(RoomDefinitions.MotherBrainSave, TourianDoorDefinitions.RinkaShaftToMotherBrainSave, new(0, 0), new(-32, 152)),
        [LoadStationDefinitions.UpperTourianSave] = new(RoomDefinitions.UpperTourianSave, TourianDoorDefinitions.TourianFirstToUpperTourianSave, new(0, 0), new(0, 152)),
        [LoadStationDefinitions.TourianFirstElevator] = new(RoomDefinitions.TourianFirst, TourianDoorDefinitions.Metroids1ToTourianFirst, new(0, 512), new(0, 168)),
        [LoadStationDefinitions.CeresElevator] = new(RoomDefinitions.CeresElevator, CeresDoorDefinitions.FallingTileToCeresElevator, new(0, 0), new(0, 72)),
    };
}
