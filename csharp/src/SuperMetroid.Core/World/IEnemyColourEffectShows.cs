using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' colour effect glows and flashes (#1512): the room's colour effect handler hands each record of an enemy's colour
/// group to the enemies that show it.
/// </summary>
internal interface IEnemyColourEffectShows
{
    /// <summary>
    /// Shows <paramref name="record"/> of <paramref name="colours"/> on the enemies that show that group: the Torizos lay
    /// their belly glow, Tourian's sprite statues their grey-out, Mother Brain and the destroyable shutters the
    /// escape's shutter flash, and the gunship its lights' glow (#1733).
    /// </summary>
    /// <exception cref="InvalidOperationException">The group names no enemy that shows it, or its owner is not loaded.</exception>
    void Show(ColourEffectColours colours, ColourEffectFrame record);
}
