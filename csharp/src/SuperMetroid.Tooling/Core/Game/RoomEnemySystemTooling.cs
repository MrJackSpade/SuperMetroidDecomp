namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="RoomEnemySystem"/>.</summary>
internal static class RoomEnemySystemToolingExtensions
{
    /// <summary>Exposes tooling-only views over room-enemy state for development and verification workflows.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Gets the indexed vertical-shutter actor slots, including null entries for unoccupied slots.</summary>
        public IReadOnlyList<VerticalShutterEnemyState?> VerticalShutterStates => self._verticalShutterStates;
    }
}
