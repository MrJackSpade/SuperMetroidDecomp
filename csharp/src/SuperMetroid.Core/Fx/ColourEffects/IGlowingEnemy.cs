namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The enemy whose placement started a glow among the room's colour effects (#1733), which shows every frame of that glow on
/// its own draws: the gunship, whose lights glow. The enemy hands itself to the glow it starts, and the glow keeps it for as
/// long as the room's colour effects run the glow (#1783): the glow lives until the room's colour effects are cleared, as
/// the original's does, and each of its frames reaches the enemy that started it, never whichever enemies the room holds then.
/// </summary>
public interface IGlowingEnemy
{
    /// <summary>Shows <paramref name="frame"/> of the glow this enemy started on its draws; each frame replaces the one before it.</summary>
    void ShowGlow(ColourEffectFrame frame);
}
