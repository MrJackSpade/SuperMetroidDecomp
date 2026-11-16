using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Brings Samus's pose history to a check's or an imported snapshot's samples through its production commit.</summary>
internal static class SamusPoseHistoryStateAccess
{
    /// <summary>Leaves the history holding the given samples.</summary>
    extension(SamusPoseHistoryState self)
    {
        /// <summary>
        /// Commits the two transitions that leave the history holding <paramref name="previousPose"/>,
        /// <paramref name="previousMovement"/> and <paramref name="lastDifferentMovement"/>: the older records the last
        /// different movement, the newer the previous pose and movement (<see cref="SamusPoseHistoryState.CommitTransition"/>).
        /// </summary>
        internal void Restore(SamusPoseId previousPose, SamusMovementType previousMovement, SamusMovementType lastDifferentMovement)
        {
            self.CommitTransition(self.PreviousPose, lastDifferentMovement);
            self.CommitTransition(previousPose, previousMovement);
        }
    }
}
