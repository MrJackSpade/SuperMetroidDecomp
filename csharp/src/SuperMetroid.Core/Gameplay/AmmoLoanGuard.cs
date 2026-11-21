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
/// count whose maximum is zero is still locked and is never changed. The loan is made to the update's Samus, whom no
/// update replaces: a saved game's load, a game's Ceres start, the arrival on Zebes and the attract demo's scenes set up a
/// new Samus between updates.
/// </remarks>
internal sealed class AmmoLoanGuard
{
    /// <summary>
    /// The loan not yet returned: lent by <see cref="Begin"/> while the assist is on, and returned once by the first
    /// <see cref="Complete"/>; none while the assist is off.
    /// </summary>
    private AmmoLoan? _outstanding;

    /// <summary>Holds the update's loan, or none when the assist is off.</summary>
    private AmmoLoanGuard(AmmoLoan? outstanding) => _outstanding = outstanding;

    /// <summary>Prepares the update's Samus's three counters before any frame logic can observe them.</summary>
    /// <param name="enabled">Whether the assist is on; when it is off, no count is changed.</param>
    /// <param name="samus">The update's Samus.</param>
    public static AmmoLoanGuard Begin(bool enabled, SamusState samus)
    {
        Ensure.NotNull(samus);
        return new AmmoLoanGuard(enabled ? AmmoLoan.Lend(samus) : null);
    }

    /// <summary>Removes any borrowed units and applies the unlocked one-unit floor, once.</summary>
    public void Complete()
    {
        // Gameplay publishes the HUD before its ordinary return, while a failing update
        // may instead unwind through an exception. Both paths call Complete;
        // returning the loan only once prevents the borrowed unit from being removed
        // twice while guaranteeing that a caught exception cannot strand the visible count
        // at the temporary value two.
        if (_outstanding is not { } loan)
            return;
        _outstanding = null;
        loan.Return();
    }

    /// <summary>The rounds one update lent the Samus it began with.</summary>
    private sealed class AmmoLoan
    {
        /// <summary>
        /// The ammunition counts the guard keeps from zero, in the order it prepares and returns them (#2557): missiles, Super
        /// Missiles and Power Bombs, each lent a round independently of the others.
        /// </summary>
        private static readonly AmmoCount[] Counts =
        [
            new(samus => samus.Missiles, samus => samus.MaxMissiles, (samus, count) => samus.Missiles = count),
            new(samus => samus.SuperMissiles, samus => samus.MaxSuperMissiles, (samus, count) => samus.SuperMissiles = count),
            new(samus => samus.PowerBombs, samus => samus.MaxPowerBombs, (samus, count) => samus.PowerBombs = count),
        ];

        /// <summary>Remembers the counts that received a temporary one-unit loan.</summary>
        private AmmoLoan(SamusState samus, IReadOnlySet<AmmoCount> lent)
        {
            Samus = samus;
            Lent = lent;
        }

        /// <summary>The Samus whose ammunition was prepared.</summary>
        private SamusState Samus { get; }
        /// <summary>The counts that began at one and were raised to two for this update.</summary>
        private IReadOnlySet<AmmoCount> Lent { get; }

        /// <summary>Raises each unlocked count of zero to one and lends a round to each unlocked count of one.</summary>
        internal static AmmoLoan Lend(SamusState samus)
        {
            var lent = new HashSet<AmmoCount>();
            foreach (AmmoCount count in Counts)
            {
                if (PrepareCounter(count.Current(samus), count.Maximum(samus), out int prepared))
                    lent.Add(count);
                count.Set(samus, prepared);
            }
            return new AmmoLoan(samus, lent);
        }

        /// <summary>Takes each lent round back and applies the unlocked one-unit floor.</summary>
        internal void Return()
        {
            foreach (AmmoCount count in Counts)
                count.Set(Samus, CompleteCounter(count.Current(Samus), count.Maximum(Samus), Lent.Contains(count)));
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

        /// <summary>One ammunition count on a Samus: how many she carries, her maximum, and how a count is written.</summary>
        /// <param name="Current">The count she carries.</param>
        /// <param name="Maximum">Her maximum, zero while the ammunition is locked.</param>
        /// <param name="Set">Writes the count she carries.</param>
        private sealed record AmmoCount(Func<SamusState, int> Current, Func<SamusState, int> Maximum, Action<SamusState, int> Set);
    }
}
