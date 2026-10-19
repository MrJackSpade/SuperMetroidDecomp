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
                !ReferenceEquals(source.Doors, self.Doors) ||
                source.BlockDefinitions.Length != self.BlockDefinitions.Length ||
                source.StreamingForegroundAllocation.Length != self.StreamingForegroundAllocation.Length ||
                source.StreamingBackgroundAllocation.Length != self.StreamingBackgroundAllocation.Length ||
                source.PlmForegroundAllocation.Length != self.PlmForegroundAllocation.Length ||
                object.ReferenceEquals(source.VisualStreamingForegroundAllocation, source.StreamingForegroundAllocation) !=
                    object.ReferenceEquals(self.VisualStreamingForegroundAllocation, self.StreamingForegroundAllocation) ||
                object.ReferenceEquals(source.VisualStreamingBackgroundAllocation, source.StreamingBackgroundAllocation) !=
                    object.ReferenceEquals(self.VisualStreamingBackgroundAllocation, self.StreamingBackgroundAllocation))
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            source.ForegroundEntries.Span.CopyTo(System.Runtime.InteropServices.MemoryMarshal.AsMemory(self.ForegroundEntries).Span);
            source.BehaviorBytes.CopyTo(self.BehaviorBytes, 0);
            source.BackgroundEntries.CopyTo(self.BackgroundEntries, 0);
            source.BlockDefinitions.Span.CopyTo(System.Runtime.InteropServices.MemoryMarshal.AsMemory(self.BlockDefinitions).Span);
            source.StreamingForegroundAllocation.CopyTo(self.StreamingForegroundAllocation, 0);
            source.StreamingBackgroundAllocation.CopyTo(self.StreamingBackgroundAllocation, 0);
            source.VisualStreamingForegroundAllocation.CopyTo(self.VisualStreamingForegroundAllocation, 0);
            source.VisualStreamingBackgroundAllocation.CopyTo(self.VisualStreamingBackgroundAllocation, 0);
            source.PlmForegroundAllocation.CopyTo(self.PlmForegroundAllocation, 0);
            source.PlmBehaviorAllocation.CopyTo(self.PlmBehaviorAllocation, 0);
            self.PendingDoorTransition = source.PendingDoorTransition;
            self.ElevatorDoorContactPending = source.ElevatorDoorContactPending;
        }
    }
}
