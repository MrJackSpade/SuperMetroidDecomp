using System.Collections.Frozen;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Selects the authored reveal motion of a Zebes area's map window (<see cref="FileSelectMapDefinitions"/>).</summary>
internal static class FileSelectMapWindowMotions
{
    /// <summary>The reveal motion of <paramref name="area"/>'s map window; the window starts at the area's installed label.</summary>
    public static FileSelectMapWindowMotion Get(ZebesArea area) => Motions[area];

    /// <summary>The reveal motion of each of the six Zebes areas' map windows.</summary>
    private static IdentityTable<ZebesArea, FileSelectMapWindowMotion> Motions => field ??= new(new Dictionary<ZebesArea, FileSelectMapWindowMotion>
    {
        [ZebesArea.Crateria] = FileSelectMapDefinitions.CrateriaReveal,
        [ZebesArea.Brinstar] = FileSelectMapDefinitions.BrinstarReveal,
        [ZebesArea.Norfair] = FileSelectMapDefinitions.NorfairReveal,
        [ZebesArea.WreckedShip] = FileSelectMapDefinitions.WreckedShipReveal,
        [ZebesArea.Maridia] = FileSelectMapDefinitions.MaridiaReveal,
        [ZebesArea.Tourian] = FileSelectMapDefinitions.TourianReveal,
    });
}
