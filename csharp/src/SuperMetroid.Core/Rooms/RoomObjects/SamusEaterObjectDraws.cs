namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The floor and ceiling plants' draws (#2034): the block shape each of the eight draws
/// (<see cref="SamusEaterObjectDrawDefinitions"/>, #1862), and the installed picture (#1557) each shows.
/// </summary>
internal static class SamusEaterObjectDraws
{
    /// <summary>The plant's frames, pictured by the installed Samus Eater frames.</summary>
    internal static RoomObjectDrawFamily<SamusEaterDraw> Family { get; } =
        new RoomObjectDrawFamily<SamusEaterDraw>("Samus Eater",
            frame => RoomObjectDrawFamily<SamusEaterDraw>.Defined(SamusEaterObjectDrawDefinitions.Shapes, frame));
}
