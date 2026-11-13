using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' palette-FX glows and flashes (#1512): the room's palette-FX handler hands each record of an enemy's colour
/// group to the enemies that show it.
/// </summary>
internal interface IEnemyPaletteFxShows
{
    /// <summary>
    /// Shows <paramref name="record"/> of <paramref name="colours"/> on the enemies that show that group: the Torizos lay
    /// their belly glow, Tourian's sprite statues their grey-out, Mother Brain and the destroyable shutters the
    /// escape's shutter flash, and the gunship its lights' glow (#1733).
    /// </summary>
    /// <exception cref="InvalidOperationException">The group names no enemy that shows it, or its owner is not loaded.</exception>
    void Show(PaletteFxColours colours, PaletteFxFrame record);
}
