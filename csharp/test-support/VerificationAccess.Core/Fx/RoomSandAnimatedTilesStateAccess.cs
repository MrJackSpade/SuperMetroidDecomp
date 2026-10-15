
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomSandAnimatedTilesState"/> members production does not use.</summary>
internal static class RoomSandAnimatedTilesStateAccess
{
    /// <summary>Counts room-selected sand animation objects after the current FX bitset is applied.</summary>
    extension(RoomSandAnimatedTilesState self)
    {
        /// <summary>Number of sand objects selected by the current room/door FX bitset.</summary>
        internal int Count => self.objects.Count;
    }
}
