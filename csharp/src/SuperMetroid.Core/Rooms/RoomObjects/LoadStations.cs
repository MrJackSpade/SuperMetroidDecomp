using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Finds a load station by the area and number a save station's room object or a save names it by.</summary>
public static class LoadStations
{
    /// <summary>The station numbered <paramref name="number"/> in <paramref name="area"/>'s station list, or null when the list numbers none.</summary>
    public static LoadStation? Find(AreaId area, int number) =>
        LoadStationDefinitions.All.SingleOrDefault(station => station.Area == area && station.Number == number);

    /// <summary>The station numbered <paramref name="number"/> in <paramref name="area"/>'s station list.</summary>
    /// <param name="area">The area whose station list holds the station.</param>
    /// <param name="number">The station's number in that list.</param>
    /// <param name="source">What named the station, reported when it names none.</param>
    /// <exception cref="InvalidDataException"><paramref name="area"/>'s station list numbers no station <paramref name="number"/>.</exception>
    public static LoadStation Get(AreaId area, int number, string source) =>
        Find(area, number) ?? throw new InvalidDataException($"{source} names {area} load station {number}, which is no load station.");
}
