using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Selected-save marker artwork anchors; neither load targets nor scroll origins are content.</summary>
/// <remarks>
/// Independently reviewed for #1165: all34 stock marker pairs in the six marker
/// views equal the load-station map projection. Calculate stock coordinates
/// through FileSelectMapLoadAnchors and retain only independently edited X/Y fields.
/// Imported artwork changes never feed back into compiled load targets or scrolling.
/// Original JSON names, coordinate bounds and serialization remain unchanged.
/// </remarks>
public sealed class MapSaveMarkerPositions
{
    /// <summary>Only X or Y fields that differ from the compiled station projection, keyed by stable marker identity.</summary>
    internal readonly Dictionary<string, AnchorOverride> coordinateOverrides;
    /// <summary>Retains validated drawing overrides independently of load targets.</summary>
    internal MapSaveMarkerPositions(Dictionary<string, AnchorOverride> coordinateOverrides) =>
        this.coordinateOverrides = coordinateOverrides;

    /// <summary>Gets the selected station's drawing anchor in area-map pixels before scroll subtraction, using each edited coordinate independently and otherwise projecting its compiled load placement.</summary>
    /// <param name="area">One of the six Zebes areas; Ceres has no save-map marker layout.</param>
    /// <param name="index">Station slot, 0..15, whose compiled load-station record has a nonzero room pointer; not an ordinal among usable markers.</param>
    /// <returns>The drawing position, without altering the station's load target or initial map scrolling.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    /// <exception cref="InvalidDataException">The station slot is unused.</exception>
    public Position16<MapPixels> Get(AreaId area, int index)
    {
        string id = MapSaveMarkers.Id(area, index);
        Position16<MapPixels> basis = FileSelectMapLoadAnchors.Get(area, index);
        var edited = coordinateOverrides.GetValueOrDefault(id);
        return new(edited.X ?? basis.X, edited.Y ?? basis.Y);
    }
}
