
namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to <see cref="RoomFloor"/> members production does not use.</summary>
internal static class RoomFloorAccess
{
    /// <summary>Exposes <see cref="RoomFloor"/> state that only verification reads.</summary>
    extension(RoomFloor self)
    {
        /// <summary>The names of the pictures drawn over the floor, in drawing order.</summary>
        public IReadOnlyList<string> Drawn => self.drawn;
    }
}
