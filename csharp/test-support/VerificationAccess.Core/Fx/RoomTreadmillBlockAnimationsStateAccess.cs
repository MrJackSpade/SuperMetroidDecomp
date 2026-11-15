
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomTreadmillBlockAnimationsState"/> members production does not use.</summary>
internal static class RoomTreadmillBlockAnimationsStateAccess
{
    /// <summary>Counts the treadmill animations the room effect started.</summary>
    extension(RoomTreadmillBlockAnimationsState self)
    {
        /// <summary>The number of treadmill animations the room effect the entry door selected started.</summary>
        internal int Count => self.animations.Count;
    }
}
