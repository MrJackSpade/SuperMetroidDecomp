namespace SuperMetroid.Core.Enemies;

/// <summary>How a hit on one part of a two-part enemy reaches the part that follows it (#2375, #2476).</summary>
internal static partial class EnemyPartnerHits
{
    /// <summary>
    /// Runs after a hit's common reaction on <paramref name="struck"/>: a dead struck part deletes
    /// <paramref name="partner"/>; a surviving one gives it its shake, invincibility, hurt flash, freeze and AI states.
    /// The partner keeps copies of them rather than reading the struck part's: the copies are observable, each part counts
    /// its copied freeze and flash down on its own turn, and a copied invincibility never counts down on a part that
    /// ignores Samus, so it keeps every later Power Bomb off that part.
    /// </summary>
    /// <param name="struck">The part the hit struck.</param>
    /// <param name="partner">The part that follows it, which the follow-up reaches.</param>
    internal static void CarryOnto(RoomEnemyPlace struck, RoomEnemyPlace partner)
    {
        Ensure.NotNull(struck);
        Ensure.NotNull(partner);
        if (struck.Health == 0)
        {
            partner.Presence = partner.Presence.With(EnemyPresence.Deleted);
            return;
        }
        partner.Shake.Restart(struck.Shake.UpdatesLeft);
        partner.Invincibility.Restart(struck.Invincibility.UpdatesLeft);
        partner.HurtFlash.Restart(struck.HurtFlash.UpdatesLeft);
        partner.Freeze.Restart(struck.Freeze.UpdatesLeft);
        partner.AiStates = struck.AiStates;
    }
}
