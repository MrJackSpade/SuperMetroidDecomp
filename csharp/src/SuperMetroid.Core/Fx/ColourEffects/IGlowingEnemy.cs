namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The enemy a colour effect among the room's colour effects shows on (#1733, #2288), which shows every frame of that effect
/// on its own draws: the gunship, whose lights glow, a Torizo, whose belly glows, and the entrance statue's Ridley and
/// Phantoon statues, which grey out. Whatever starts the effect hands it the enemy, and the effect keeps it for as long as
/// the room's colour effects run it (#1783): each of its frames reaches that enemy, never whichever enemies the room holds
/// then.
/// </summary>
public interface IGlowingEnemy
{
    /// <summary>Shows <paramref name="frame"/> of the glow this enemy started on its draws; each frame replaces the one before it.</summary>
    void ShowGlow(ColourEffectFrame frame);
}
