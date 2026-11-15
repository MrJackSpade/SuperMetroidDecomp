
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomSandBlockAnimationsState"/> members production does not use.</summary>
internal static class RoomSandBlockAnimationsStateAccess
{
    /// <summary>Counts the sand animations the room effect started.</summary>
    extension(RoomSandBlockAnimationsState self)
    {
        /// <summary>The number of sand animations the room effect the entry door selected started.</summary>
        internal int Count => self.animations.Count;
    }
}
