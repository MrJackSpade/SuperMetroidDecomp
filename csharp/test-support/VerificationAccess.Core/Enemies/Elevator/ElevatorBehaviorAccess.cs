namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Verification access to <see cref="ElevatorBehavior"/> members production does not use.</summary>
internal static class ElevatorBehaviorAccess
{
    /// <summary>Exposes <see cref="ElevatorBehavior"/> state that only verification reads.</summary>
    extension(ElevatorBehavior self)
    {
        /// <summary>
        /// Debugger/test seam that puts the ride in <paramref name="status"/>; ending the ride also clears its journey
        /// flags. Production changes are still made by collision, actor AI, and room loading rather than arbitrary callers.
        /// </summary>
        public void SetElevatorStatusForDebugging(ElevatorActorStatus status)
        {
            self.ElevatorStatus = status;
            if (self.ElevatorStatus == ElevatorActorStatus.Inactive)
                self.ElevatorFlags = ElevatorJourneyFlag.None;
        }
    }
}
