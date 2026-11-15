namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The escape animals' rescue walls (#1862, #2034): the column each of the four draws lays, from
/// <see cref="EscapeAnimalObjectDrawDefinitions"/>, and the installed picture each shows.
/// </summary>
internal static class EscapeAnimalWalls
{
    /// <summary>The wall's four frames, pictured by the installed reveals and walls.</summary>
    internal static RoomObjectDrawFamily<EscapeAnimalDraw> Family { get; } =
        RoomObjectDrawFamily<EscapeAnimalDraw>.Pictured<InstalledRoomObjectRevealAndWallVisuals>("reveal and wall",
            frame => RoomObjectDrawFamily<EscapeAnimalDraw>.Defined(EscapeAnimalObjectDrawDefinitions.Shapes, frame),
            (visuals, draw) => visuals.Image(draw));
}
