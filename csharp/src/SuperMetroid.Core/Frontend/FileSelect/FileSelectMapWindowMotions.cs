using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Selects the authored reveal motion of a Zebes area's map window (<see cref="FileSelectMapDefinitions"/>).</summary>
internal static partial class FileSelectMapWindowMotions
{
    /// <summary>The reveal motion of <paramref name="area"/>'s map window; the window starts at the area's installed label.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is Ceres, which has no world-map window.</exception>
    public static FileSelectMapWindowMotion Get(AreaId area) => area switch
    {
        AreaId.Crateria => FileSelectMapDefinitions.CrateriaReveal,
        AreaId.Brinstar => FileSelectMapDefinitions.BrinstarReveal,
        AreaId.Norfair => FileSelectMapDefinitions.NorfairReveal,
        AreaId.WreckedShip => FileSelectMapDefinitions.WreckedShipReveal,
        AreaId.Maridia => FileSelectMapDefinitions.MaridiaReveal,
        AreaId.Tourian => FileSelectMapDefinitions.TourianReveal,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas have world-map transitions."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas have world-map transitions."),
    };
}
