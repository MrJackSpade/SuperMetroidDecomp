using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool instance members of <see cref="RoomLevelData"/>.</summary>
internal static class RoomLevelDataToolingExtensions
{
    extension(RoomLevelData self)
    {
        /// <summary>Read-only parallel BTS (“block type special”) byte plane.</summary>
        public ReadOnlyMemory<byte> BehaviorBytes => self._behaviorBytes;
    }
}
