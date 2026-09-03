using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

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
internal readonly record struct HostInfiniteAmmoFrameGuard(
    SamusState? Actor,
    bool MissilesBuffered,
    bool SuperMissilesBuffered,
    bool PowerBombsBuffered)
{
    /// <summary>Prepares the three counters before any frame logic can observe them.</summary>
    public static HostInfiniteAmmoFrameGuard Begin(bool enabled, SamusState? samus)
    {
        if (!enabled || samus is null)
            return default;

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
