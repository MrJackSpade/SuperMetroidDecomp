using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemies' colour effect glows and flashes (#1512): the room's colour effects hand each frame of an enemy's colour
/// group to the enemies that show it.
/// </summary>
internal interface IEnemyColourEffectShows
{
    /// <summary>
    /// Shows <paramref name="frame"/> of <paramref name="colours"/> on the enemies that show that group: the Torizos lay
    /// their belly glow, Tourian's sprite statues their grey-out, and the destroyable shutters the escape's shutter flash.
    /// The gunship's glow shows on the gunship that started it, never through the room's enemies (#1783), and Mother
    /// Brain's exploded door's fragments keep their own colours (#1544).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The group names no enemy that shows it or is the gunship's glow, or its owner is not loaded.
    /// </exception>
    void Show(ColourEffectColours colours, ColourEffectFrame frame);
}
