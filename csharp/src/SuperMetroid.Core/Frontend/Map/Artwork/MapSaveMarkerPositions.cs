using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The installed selected-save marker drawing anchors; neither load targets nor scroll origins are content. The
/// installation is the anchors' only source (#1596): imported artwork changes never feed back into compiled load targets or
/// scrolling.
/// </summary>
public sealed class MapSaveMarkerPositions
{
    /// <summary>The installed drawing anchor of each marker, keyed by stable marker identity.</summary>
    internal readonly Dictionary<string, Position16<MapPixels>> anchors;

    /// <summary>Keeps the complete validated drawing anchors, independently of load targets.</summary>
    /// <param name="anchors">Every supported marker's anchor, keyed by stable marker identity.</param>
    internal MapSaveMarkerPositions(IReadOnlyDictionary<string, Position16<MapPixels>> anchors) =>
        this.anchors = new Dictionary<string, Position16<MapPixels>>(Ensure.NotNull(anchors), StringComparer.Ordinal);

    /// <summary>Gets the selected station's installed drawing anchor in area-map pixels before scroll subtraction.</summary>
    /// <param name="station">A station with a map marker.</param>
    /// <returns>The drawing position, without altering the station's load target or initial map scrolling.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The station has no map marker.</exception>
    /// <exception cref="InvalidDataException">The station's marker is not installed.</exception>
    public Position16<MapPixels> Get(LoadStation station)
    {
        string id = MapSaveMarkers.Id(station);
        return anchors.TryGetValue(id, out Position16<MapPixels> anchor) ? anchor
            : throw new InvalidDataException($"Save marker {id} is not installed.");
    }
}
