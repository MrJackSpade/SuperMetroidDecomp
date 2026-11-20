using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's door block draws (#2034): the block shape each draws, from <see cref="PhantoonDoorObjectDrawDefinitions"/>,
/// and the installed picture each shows.
/// </summary>
internal static class PhantoonDoorDraws
{
    /// <summary>The door's frames, pictured by the installed Phantoon door frames.</summary>
    internal static RoomObjectDrawFamily<PhantoonDoorDraw> Family { get; } =
        new RoomObjectDrawFamily<PhantoonDoorDraw>("Phantoon door",
            frame => RoomObjectDrawFamily<PhantoonDoorDraw>.Defined(PhantoonDoorObjectDrawDefinitions.Shapes, frame));
}
