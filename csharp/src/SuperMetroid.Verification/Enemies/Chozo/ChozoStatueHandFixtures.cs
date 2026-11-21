using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>
/// The hand a placed Chozo statue carries (#2554), for checks that spawn a statue's hand in a room of room objects alone:
/// the statue's setup builds it with its room's statue behaviour, which wakes the statue once the hand is triggered.
/// </summary>
internal static class ChozoStatueHandFixtures
{
    /// <summary>The hand of a statue placed at <paramref name="site"/>, with the state the hand wakes.</summary>
    /// <param name="site">The statue's site.</param>
    /// <param name="statue">The placed statue's state, which a triggered hand wakes.</param>
    internal static ChozoStatueHandTrigger Placed(ChozoStatueSite site, out ChozoStatueState statue)
    {
        statue = new ChozoStatueState { Phase = ChozoStatuePhase.Placed, Site = site };
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        return new ChozoStatueHandTrigger(enemies.ChozoStatueBehavior, statue);
    }

    /// <summary>The hand of a statue placed at <paramref name="site"/>.</summary>
    internal static ChozoStatueHandTrigger Placed(ChozoStatueSite site) => Placed(site, out _);
}
