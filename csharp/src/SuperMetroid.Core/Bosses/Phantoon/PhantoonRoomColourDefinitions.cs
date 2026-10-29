using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The background colours Phantoon's death fades (#1405, #1464).</summary>
internal static class PhantoonRoomColourDefinitions
{
    /// <summary>
    /// The room sets that fade to the Wrecked Ship's powered-on colours: every set but the sixth tile set, which holds
    /// Phantoon's own body, in the order the installed powered-on colours hold them, sixteen colours each.
    /// </summary>
    internal static readonly BackgroundColourSet[] PowerOn =
    [
        BackgroundColourSet.Hud,
        BackgroundColourSet.HudItemsAndEffects,
        BackgroundColourSet.TilesFirst,
        BackgroundColourSet.TilesSecond,
        BackgroundColourSet.TilesThird,
        BackgroundColourSet.TilesFourth,
        BackgroundColourSet.TilesFifth,
    ];
}
