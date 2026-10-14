namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="RoomEnemySystem"/>.</summary>
internal static class RoomEnemySystemToolingExtensions
{
    /// <summary>Exposes the shutter species' private state through the owning enemy system for development tools.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Returns the native-slot-indexed shutter states, including empty slots.</summary>
        public IReadOnlyList<VerticalShutterEnemyState?> VerticalShutterStates => self.ShuttersBehavior._verticalShutterStates;
    }
}
