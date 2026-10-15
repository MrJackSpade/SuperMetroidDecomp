using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Frame-local host guard which prevents unlocked ammunition from reaching observable zero.
/// </summary>
/// <remarks>
/// Cartridge firing routines decrement their counters and immediately use zero to cancel the
/// selected HUD item. Merely repairing zero at the end of the frame would therefore require
/// the player to reselect the weapon after every final shot. This guard lends a one-unit
/// buffer to an unlocked counter that begins at one, lets the complete cartridge frame run,
/// then removes that loan. Locked counters are identified solely by their zero maximum and
/// are never changed.
/// </remarks>
internal sealed class HostInfiniteAmmoFrameGuard
{
    /// <summary>Prevents a borrowed ammo unit from being removed twice on normal and error exits.</summary>
    private bool _completed;

    /// <summary>Remembers which actor and counters received a temporary one-unit loan.</summary>
    private HostInfiniteAmmoFrameGuard(
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

    /// <summary>Exact Samus instance whose ammo was prepared, if the option was active.</summary>
    private SamusState? Actor { get; }
    /// <summary>Whether missiles began at one and were raised to two for this frame.</summary>
    private bool MissilesBuffered { get; }
    /// <summary>Whether super missiles began at one and were raised to two for this frame.</summary>
    private bool SuperMissilesBuffered { get; }
    /// <summary>Whether power bombs began at one and were raised to two for this frame.</summary>
    private bool PowerBombsBuffered { get; }

    /// <summary>Prepares the three counters before any frame logic can observe them.</summary>
    public static HostInfiniteAmmoFrameGuard Begin(bool enabled, SamusState? samus)
    {
        if (!enabled || samus is null)
            return new HostInfiniteAmmoFrameGuard(null, false, false, false);

        bool missilesBuffered = PrepareCounter(
            samus.Missiles,
            samus.MaxMissiles,
            out ushort preparedMissiles);
        samus.Missiles = preparedMissiles;
        bool superMissilesBuffered = PrepareCounter(
            samus.SuperMissiles,
            samus.MaxSuperMissiles,
            out ushort preparedSuperMissiles);
        samus.SuperMissiles = preparedSuperMissiles;
        bool powerBombsBuffered = PrepareCounter(
            samus.PowerBombs,
            samus.MaxPowerBombs,
            out ushort preparedPowerBombs);
        samus.PowerBombs = preparedPowerBombs;
        return new HostInfiniteAmmoFrameGuard(
            samus,
            missilesBuffered,
            superMissilesBuffered,
            powerBombsBuffered);
    }

    /// <summary>Removes any borrowed units and applies the unlocked one-unit floor.</summary>
    public void Complete(SamusState? currentActor)
    {
        // Gameplay publishes the HUD before its ordinary return, while the host error
        // boundary may instead unwind through an exception. Both paths call Complete;
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
        ushort current,
        ushort maximum,
        out ushort prepared)
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
    private static ushort CompleteCounter(ushort current, ushort maximum, bool buffered)
    {
        if (maximum == 0)
            return current;
        if (!buffered)
            return current == 0 ? (ushort)1 : current;

        // Removing the loan from a count of one would recreate the forbidden zero. This is
        // the final-shot case: the cartridge consumed the borrowed unit during this frame.
        return current <= 1 ? (ushort)1 : unchecked((ushort)(current - 1));
    }
}
