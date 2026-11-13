
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomTreadmillBlockAnimationsState"/> members production does not use.</summary>
internal static class RoomTreadmillAnimatedTilesStateAccess
{
    /// <summary>Counts treadmill animation objects selected by the room or door FX record.</summary>
    extension(RoomTreadmillBlockAnimationsState self)
    {
        /// <summary>Number of treadmill objects selected by the current room/door FX bitset.</summary>
        internal int Count => self.objects.Count;
    }
}
