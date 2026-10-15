namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Development-tool instance members of <see cref="RoomLevelData"/>.</summary>
internal static class RoomLevelDataToolingExtensions
{
    /// <summary>Gives development tools read-only access to the room's parallel block behavior plane.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>Read-only parallel BTS (“block type special”) byte plane.</summary>
        public ReadOnlyMemory<byte> BehaviorBytes => self._behaviorBytes;
    }
}
