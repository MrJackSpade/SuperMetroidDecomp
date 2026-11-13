using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The numbers each area's load-station list gives its load stations, which the save files keep a checkpoint and the
/// used-station record by, and which room data names a save station's load station with. The save codecs and the import
/// number stations here; the game names stations and never sees the number.
/// </summary>
public static class LoadStationNumberCodec
{
    /// <summary>Each load station's number in its area's station list.</summary>
    private static readonly Dictionary<LoadStation, int> Numbers = new()
    {
        [LoadStationDefinitions.Gunship] = 0,
        [LoadStationDefinitions.CrateriaSave] = 1,
        [LoadStationDefinitions.ForgottenHighwayElevator] = 8,
        [LoadStationDefinitions.RedBrinstarElevator] = 9,
        [LoadStationDefinitions.BlueBrinstarElevator] = 10,
        [LoadStationDefinitions.GreenBrinstarElevator] = 11,
        [LoadStationDefinitions.StatuesElevator] = 12,
        [LoadStationDefinitions.ZebesArrival] = 18,
        [LoadStationDefinitions.BigPinkSave] = 0,
        [LoadStationDefinitions.GreenBrinstarSave] = 1,
        [LoadStationDefinitions.EtecoonSave] = 2,
        [LoadStationDefinitions.WarehouseSave] = 3,
        [LoadStationDefinitions.RedBrinstarSave] = 4,
        [LoadStationDefinitions.GreenBrinstarMainShaftElevator] = 8,
        [LoadStationDefinitions.MorphBallElevator] = 9,
        [LoadStationDefinitions.CaterpillarElevator] = 10,
        [LoadStationDefinitions.WarehouseEntranceElevator] = 11,
        [LoadStationDefinitions.PostCrocomireSave] = 0,
        [LoadStationDefinitions.BubbleMountainSave] = 1,
        [LoadStationDefinitions.FrogSave] = 2,
        [LoadStationDefinitions.CrocomireSave] = 3,
        [LoadStationDefinitions.LowerNorfairElevatorSave] = 4,
        [LoadStationDefinitions.LowerNorfairSave] = 5,
        [LoadStationDefinitions.BusinessCenterElevator] = 8,
        [LoadStationDefinitions.LowerNorfairElevator] = 9,
        [LoadStationDefinitions.MainHallElevator] = 10,
        [LoadStationDefinitions.WreckedShipSave] = 0,
        [LoadStationDefinitions.GlassTunnelSave] = 0,
        [LoadStationDefinitions.ForgottenHighwaySave] = 1,
        [LoadStationDefinitions.AqueductSave] = 2,
        [LoadStationDefinitions.DraygonSave] = 3,
        [LoadStationDefinitions.MaridiaElevator] = 8,
        [LoadStationDefinitions.MotherBrainSave] = 0,
        [LoadStationDefinitions.UpperTourianSave] = 1,
        [LoadStationDefinitions.TourianFirstElevator] = 8,
        [LoadStationDefinitions.CeresElevator] = 0,
    };

    /// <summary>The number <paramref name="station"/>'s area's station list gives it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="station"/> is not a load station.</exception>
    public static int Number(LoadStation station) =>
        Numbers.TryGetValue(Ensure.NotNull(station), out int number)
            ? number
            : throw new ArgumentOutOfRangeException(nameof(station), station, "Not a load station.");

    /// <summary>The station numbered <paramref name="number"/> in <paramref name="area"/>'s station list, or null when the list numbers none.</summary>
    public static LoadStation? Find(AreaId area, int number) =>
        LoadStationDefinitions.All.SingleOrDefault(station => station.Area == area && Numbers[station] == number);

    /// <summary>The station numbered <paramref name="number"/> in <paramref name="area"/>'s station list.</summary>
    /// <param name="area">The area whose station list holds the station.</param>
    /// <param name="number">The station's number in that list.</param>
    /// <param name="source">What named the station, reported when it names none.</param>
    /// <exception cref="InvalidDataException"><paramref name="area"/>'s station list numbers no station <paramref name="number"/>.</exception>
    public static LoadStation Decode(AreaId area, int number, string source) =>
        Find(area, number) ?? throw new InvalidDataException($"{source} names {area} load station {number}, which is no load station.");
}
