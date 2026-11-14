
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomSandBlockAnimationsState"/> members production does not use.</summary>
internal static class RoomSandBlockAnimationsStateAccess
{
    /// <summary>Counts room-selected sand animation objects after the current FX bitset is applied.</summary>
    extension(RoomSandBlockAnimationsState self)
    {
        /// <summary>Number of sand objects selected by the current room/door FX bitset.</summary>
        internal int Count => self.objects.Count;
    }
}
