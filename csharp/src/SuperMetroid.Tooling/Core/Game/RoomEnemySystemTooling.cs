namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="RoomEnemySystem"/>.</summary>
internal static class RoomEnemySystemToolingExtensions
{
    /// <summary>Exposes vertical shutter state from <see cref="RoomEnemySystem"/> for development tools.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Gets vertical shutter states.</summary>
        public IReadOnlyList<VerticalShutterEnemyState?> VerticalShutterStates => self.ShuttersBehavior._verticalShutterStates;
    }
}
