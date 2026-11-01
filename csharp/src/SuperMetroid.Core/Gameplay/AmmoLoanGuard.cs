using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The <see cref="GameplayAssists.AmmoNeverRunsOut"/> assist for one update: it keeps each unlocked ammunition count
/// from ever being seen at zero.
/// </summary>
/// <remarks>
/// Firing decrements a count and immediately uses zero to cancel the selected HUD item, so merely refilling zero at the
/// end of the update would make the player reselect the weapon after every last round. The guard instead lends one
/// round to an unlocked count that begins the update at one, lets the whole update run, then takes the loan back. A
/// count whose maximum is zero is still locked and is never changed.
/// </remarks>
internal sealed class AmmoLoanGuard
{
    /// <summary>Prevents a borrowed ammo unit from being removed twice on normal and error exits.</summary>
    private bool _completed;

    /// <summary>Remembers which actor and counters received a temporary one-unit loan.</summary>
    [IndependentFlag("Each ammunition type is lent a round independently of the others.")]
    private AmmoLoanGuard(
        SamusState? actor,
        bool missilesBuffered,
        bool superMissilesBuffered,
        bool powerBombsBuffered)
    {
        Actor = actor;
        MissilesBuffered = missilesBuffered;
        SuperMissilesBuffered = superMissilesBuffered;
        PowerBombsBuffered = powerBombsBuffered;
    }

    /// <summary>The exact Samus whose ammunition was prepared, if the assist was active.</summary>
    private SamusState? Actor { get; }
    /// <summary>Whether missiles began at one and were raised to two for this frame.</summary>
    private bool MissilesBuffered { get; }
    /// <summary>Whether super missiles began at one and were raised to two for this frame.</summary>
    private bool SuperMissilesBuffered { get; }
    /// <summary>Whether power bombs began at one and were raised to two for this frame.</summary>
    private bool PowerBombsBuffered { get; }

    /// <summary>Prepares the three counters before any frame logic can observe them.</summary>
    public static AmmoLoanGuard Begin(bool enabled, SamusState? samus)
    {
        if (!enabled || samus is null)
            return new AmmoLoanGuard(null, false, false, false);

        bool missilesBuffered = PrepareCounter(
            samus.Missiles,
            samus.MaxMissiles,
            out int preparedMissiles);
        samus.Missiles = preparedMissiles;
        bool superMissilesBuffered = PrepareCounter(
            samus.SuperMissiles,
            samus.MaxSuperMissiles,
            out int preparedSuperMissiles);
        samus.SuperMissiles = preparedSuperMissiles;
        bool powerBombsBuffered = PrepareCounter(
            samus.PowerBombs,
            samus.MaxPowerBombs,
            out int preparedPowerBombs);
        samus.PowerBombs = preparedPowerBombs;
        return new AmmoLoanGuard(
            samus,
            missilesBuffered,
            superMissilesBuffered,
            powerBombsBuffered);
    }

    /// <summary>Removes any borrowed units and applies the unlocked one-unit floor.</summary>
    public void Complete(SamusState? currentActor)
    {
        // Gameplay publishes the HUD before its ordinary return, while a failing update
        // may instead unwind through an exception. Both paths call Complete;
        // making reconciliation idempotent prevents the borrowed unit from being removed
        // twice while guaranteeing that a caught exception cannot strand the visible count
        // at the temporary value two.
        if (_completed)
            return;
        _completed = true;

        // Room loading can replace Samus during a frame. A loan belongs to the exact actor
        // that received it and must never be reconciled against a newly loaded save/room actor.
        if (Actor is null || !ReferenceEquals(Actor, currentActor))
            return;

        Actor.Missiles = CompleteCounter(
            Actor.Missiles,
            Actor.MaxMissiles,
            MissilesBuffered);
        Actor.SuperMissiles = CompleteCounter(
            Actor.SuperMissiles,
            Actor.MaxSuperMissiles,
            SuperMissilesBuffered);
        Actor.PowerBombs = CompleteCounter(
            Actor.PowerBombs,
            Actor.MaxPowerBombs,
            PowerBombsBuffered);
    }

    /// <summary>Raises unlocked zero to one or lends a unit to an unlocked count of one.</summary>
    private static bool PrepareCounter(
        int current,
        int maximum,
        out int prepared)
    {
        prepared = current;
        if (maximum == 0)
            return false;
        if (current == 0)
        {
            prepared = 1;
            return false;
        }
        if (current != 1)
            return false;

        prepared = 2;
        return true;
    }

    /// <summary>Returns a borrowed unit while preserving an unlocked floor of one.</summary>
    private static int CompleteCounter(int current, int maximum, bool buffered)
    {
        if (maximum == 0)
            return current;
        if (!buffered)
            return current == 0 ? 1 : current;

        // Removing the loan from a count of one would recreate the forbidden zero. This is
        // the final-shot case: the game consumed the borrowed unit during this frame.
        return current <= 1 ? 1 : current - 1;
    }
}
