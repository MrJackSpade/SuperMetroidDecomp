namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="RoomEnemySystem"/>.</summary>
internal static class RoomEnemySystemToolingExtensions
{
    extension(RoomEnemySystem self)
    {
        public IReadOnlyList<VerticalShutterEnemyState?> VerticalShutterStates => self._verticalShutterStates;
    }
}
