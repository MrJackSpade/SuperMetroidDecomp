namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Restores mutable room level entries and behavior data from a fixture snapshot.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// Returns this level to <paramref name="source"/>'s complete state in place: every block
        /// plane, allocation tail and pending door flag. Both levels must share one layout.
        /// </summary>
        internal void RestoreFrom(RoomLevelData source)
        {
            Ensure.NotNull(source);
            if (source.Size.X != self.Size.X || source.Size.Y != self.Size.Y ||
                source.DoorListPointer != self.DoorListPointer ||
                source._blockDefinitions.Length != self._blockDefinitions.Length ||
                source._streamingForegroundAllocation.Length != self._streamingForegroundAllocation.Length ||
                source._streamingBackgroundAllocation.Length != self._streamingBackgroundAllocation.Length ||
                source._plmForegroundAllocation.Length != self._plmForegroundAllocation.Length ||
                object.ReferenceEquals(source._visualStreamingForegroundAllocation, source._streamingForegroundAllocation) !=
                    object.ReferenceEquals(self._visualStreamingForegroundAllocation, self._streamingForegroundAllocation) ||
                object.ReferenceEquals(source._visualStreamingBackgroundAllocation, source._streamingBackgroundAllocation) !=
                    object.ReferenceEquals(self._visualStreamingBackgroundAllocation, self._streamingBackgroundAllocation))
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            source._foregroundEntries.CopyTo(self._foregroundEntries, 0);
            source._behaviorBytes.CopyTo(self._behaviorBytes, 0);
            source._backgroundEntries.CopyTo(self._backgroundEntries, 0);
            source._blockDefinitions.CopyTo(self._blockDefinitions, 0);
            source._streamingForegroundAllocation.CopyTo(self._streamingForegroundAllocation, 0);
            source._streamingBackgroundAllocation.CopyTo(self._streamingBackgroundAllocation, 0);
            source._visualStreamingForegroundAllocation.CopyTo(self._visualStreamingForegroundAllocation, 0);
            source._visualStreamingBackgroundAllocation.CopyTo(self._visualStreamingBackgroundAllocation, 0);
            source._plmForegroundAllocation.CopyTo(self._plmForegroundAllocation, 0);
            source._plmBehaviorAllocation.CopyTo(self._plmBehaviorAllocation, 0);
            self.PendingDoorTransition = source.PendingDoorTransition;
            self.ElevatorDoorContactPending = source.ElevatorDoorContactPending;
        }
    }
}
