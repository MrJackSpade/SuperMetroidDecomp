using System.Collections.Frozen;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Selects the authored reveal motion of a Zebes area's map window (<see cref="FileSelectMapDefinitions"/>).</summary>
internal static class FileSelectMapWindowMotions
{
    /// <summary>The reveal motion of <paramref name="area"/>'s map window; the window starts at the area's installed label.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is Ceres, which has no world-map window.</exception>
    public static FileSelectMapWindowMotion Get(AreaId area) => Ensure.Listed(Motions, area);

    /// <summary>The reveal motion of each of the six Zebes areas' map windows.</summary>
    private static FrozenDictionary<AreaId, FileSelectMapWindowMotion> Motions => field ??= new Dictionary<AreaId, FileSelectMapWindowMotion>
    {
        [AreaId.Crateria] = FileSelectMapDefinitions.CrateriaReveal,
        [AreaId.Brinstar] = FileSelectMapDefinitions.BrinstarReveal,
        [AreaId.Norfair] = FileSelectMapDefinitions.NorfairReveal,
        [AreaId.WreckedShip] = FileSelectMapDefinitions.WreckedShipReveal,
        [AreaId.Maridia] = FileSelectMapDefinitions.MaridiaReveal,
        [AreaId.Tourian] = FileSelectMapDefinitions.TourianReveal,
    }.ToFrozenDictionary();
}
