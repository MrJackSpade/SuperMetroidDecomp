using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where loading at each load station puts Samus (<see cref="LoadStationPlacementDefinitions"/>).</summary>
public static class LoadStationPlacements
{
    /// <summary>A load station's placement.</summary>
    extension(LoadStation station)
    {
        /// <summary>Where loading at the station puts Samus.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The station is not a load station.</exception>
        public LoadStationPlacement Placement =>
            LoadStationPlacementDefinitions.ByStation.TryGetValue(Ensure.NotNull(station), out LoadStationPlacement? placement)
                ? placement
                : throw new ArgumentOutOfRangeException(nameof(station), station, "Not a load station.");
    }
}
