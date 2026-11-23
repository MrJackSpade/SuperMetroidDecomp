using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// A corpse's wait to rot (#2557), which every corpse Samus stands on shares: once she collides with the solid corpse its
/// pre-rot delay counts (<see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/>), and when the delay runs out the
/// corpse ignores Samus from then on and starts rotting.
/// </summary>
internal static partial class CorpseRotWait
{
    /// <summary>
    /// One update of the wait of the corpse at <paramref name="place"/> in <paramref name="phase"/>, its delay
    /// <paramref name="preRotDelay"/>; the phase it runs next.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse is no longer waiting to rot.</exception>
    internal static CorpsePhase Step(CorpsePhase phase, RoomEnemyPlace place, SamusState samus, UpdateCountdown preRotDelay)
    {
        switch (phase)
        {
            case CorpsePhase.Waiting:
                return samus.Kinematics.DidCollideWithSolidEnemy(place) == true ? CorpsePhase.PreRotDelay : phase;
            case CorpsePhase.PreRotDelay:
                if (!preRotDelay.Step())
                    return phase;
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                return CorpsePhase.Rotting;
            case CorpsePhase.Rotting:
            case CorpsePhase.Finished:
            default:
                throw new ArgumentOutOfRangeException(nameof(phase), phase, "The corpse is no longer waiting to rot.");
        }
    }
}
