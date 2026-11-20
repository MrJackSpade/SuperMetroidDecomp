namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The elevator platform's block draws (#1862, #2034): the block shape each frame draws, from
/// <see cref="ElevatorPlatformObjectDefinitions"/>, and the installed picture each shows.
/// </summary>
internal static class ElevatorPlatformDraws
{
    /// <summary>The platform's three frames, pictured by the installed elevator platform frames.</summary>
    internal static RoomObjectDrawFamily<ElevatorPlatformDraw> Family { get; } =
        new RoomObjectDrawFamily<ElevatorPlatformDraw>("elevator-platform",
            frame => RoomObjectDrawFamily<ElevatorPlatformDraw>.Defined(ElevatorPlatformObjectDefinitions.Shapes, frame));
}
