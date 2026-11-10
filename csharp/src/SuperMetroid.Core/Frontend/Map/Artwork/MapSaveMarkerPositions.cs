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
    /// <param name="area">One of the six Zebes areas; Ceres has no save-map marker layout.</param>
    /// <param name="index">Station slot, 0..15, whose compiled load-station record names a room; not an ordinal among usable markers.</param>
    /// <returns>The drawing position, without altering the station's load target or initial map scrolling.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    /// <exception cref="InvalidDataException">The station slot has no installed marker.</exception>
    public Position16<MapPixels> Get(AreaId area, int index)
    {
        string id = MapSaveMarkers.Id(area, index);
        return anchors.TryGetValue(id, out Position16<MapPixels> anchor) ? anchor
            : throw new InvalidDataException($"Save marker {id} is not installed.");
    }
}
