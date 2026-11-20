using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where loading at each load station puts Samus: the door its room is entered through, the initial camera and Samus's
/// room position.
/// </summary>
internal static class LoadStationPlacementDefinitions
{
    /// <summary>Each load station's placement.</summary>
    internal static IReadOnlyDictionary<LoadStation, LoadStationPlacement> ByStation { get; } = new Dictionary<LoadStation, LoadStationPlacement>
    {
        [LoadStationDefinitions.Gunship] = new(CrateriaDoorDefinitions.ParlorToLandingSite, new(1024, 1024), new(1152, 1088)),
        [LoadStationDefinitions.CrateriaSave] = new(CrateriaDoorDefinitions.ParlorToCrateriaSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.ForgottenHighwayElevator] = new(CrateriaDoorDefinitions.ForgottenHighwayElbowToForgottenHighwayElevator, new(0, 0), new(128, 168)),
        [LoadStationDefinitions.RedBrinstarElevator] = new(CrateriaDoorDefinitions.CrateriaKihunterToRedBrinstarElevator, new(0, 0), new(128, 168)),
        [LoadStationDefinitions.BlueBrinstarElevator] = new(CrateriaDoorDefinitions.PitToElevatorToBlueBrinstar, new(0, 0), new(128, 136)),
        [LoadStationDefinitions.GreenBrinstarElevator] = new(CrateriaDoorDefinitions.LowerMushroomsToGreenBrinstarElevator, new(0, 0), new(128, 136)),
        [LoadStationDefinitions.StatuesElevator] = new(CrateriaDoorDefinitions.StatuesHallwayToStatues, new(0, 256), new(128, 408)),
        [LoadStationDefinitions.ZebesArrival] = new(CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(1024, 0), new(1152, 128)),
        [LoadStationDefinitions.BigPinkSave] = new(BrinstarDoorDefinitions.BigPinkToBigPinkSaveRoom, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.GreenBrinstarSave] = new(BrinstarDoorDefinitions.GreenBrinstarMainShaftToGreenBrinstarSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.EtecoonSave] = new(BrinstarDoorDefinitions.EtecoonEnergyTankToEtecoonSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.WarehouseSave] = new(BrinstarDoorDefinitions.WarehouseKihunterToWarehouseSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.RedBrinstarSave] = new(BrinstarDoorDefinitions.CaterpillarToRedBrinstarSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.GreenBrinstarMainShaftElevator] = new(BrinstarDoorDefinitions.BrinstarPreMapToGreenBrinstarMainShaft, new(0, 512), new(128, 680)),
        [LoadStationDefinitions.MorphBallElevator] = new(BrinstarDoorDefinitions.GreenHillZoneToMorphBall, new(1280, 512), new(1408, 680)),
        [LoadStationDefinitions.CaterpillarElevator] = new(BrinstarDoorDefinitions.HellwayToCaterpillar, new(0, 512), new(128, 680)),
        [LoadStationDefinitions.WarehouseEntranceElevator] = new(MaridiaDoorDefinitions.EastTunnelToWarehouseEntrance, new(0, 0), new(128, 136)),
        [LoadStationDefinitions.PostCrocomireSave] = new(NorfairDoorDefinitions.PostCrocomireFarmingToPostCrocomireSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.BubbleMountainSave] = new(NorfairDoorDefinitions.BubbleMountainToBubbleMountainSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.FrogSave] = new(NorfairDoorDefinitions.FrogSpeedwayToFrogSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.CrocomireSave] = new(NorfairDoorDefinitions.CrocomireSpeedwayToCrocomireSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.LowerNorfairElevatorSave] = new(NorfairDoorDefinitions.LowerNorfairElevatorToLowerNorfairElevatorSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.LowerNorfairSave] = new(NorfairDoorDefinitions.RedKihunterShaftToLowerNorfairSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.BusinessCenterElevator] = new(NorfairDoorDefinitions.CathedralEntranceToBusinessCenter, new(0, 512), new(128, 680)),
        [LoadStationDefinitions.LowerNorfairElevator] = new(NorfairDoorDefinitions.LavaDiveToLowerNorfairElevator, new(0, 0), new(128, 136)),
        [LoadStationDefinitions.MainHallElevator] = new(NorfairDoorDefinitions.AcidStatueToMainHall, new(1024, 512), new(1152, 648)),
        [LoadStationDefinitions.WreckedShipSave] = new(WreckedShipDoorDefinitions.WreckedShipMainShaftToWreckedShipSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.GlassTunnelSave] = new(MaridiaDoorDefinitions.GlassTunnelToGlassTunnelSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.ForgottenHighwaySave] = new(MaridiaDoorDefinitions.MaridiaElevatorToForgottenHighwaySave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.AqueductSave] = new(MaridiaDoorDefinitions.AqueductToAqueductSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.DraygonSave] = new(MaridiaDoorDefinitions.ColosseumToDraygonSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.MaridiaElevator] = new(MaridiaDoorDefinitions.ThreadTheNeedleToMaridiaElevator, new(0, 512), new(128, 680)),
        [LoadStationDefinitions.MotherBrainSave] = new(TourianDoorDefinitions.RinkaShaftToMotherBrainSave, new(0, 0), new(96, 152)),
        [LoadStationDefinitions.UpperTourianSave] = new(TourianDoorDefinitions.TourianFirstToUpperTourianSave, new(0, 0), new(128, 152)),
        [LoadStationDefinitions.TourianFirstElevator] = new(TourianDoorDefinitions.Metroids1ToTourianFirst, new(0, 512), new(128, 680)),
        [LoadStationDefinitions.CeresElevator] = new(CeresDoorDefinitions.FallingTileToCeresElevator, new(0, 0), new(128, 72)),
    };
}
