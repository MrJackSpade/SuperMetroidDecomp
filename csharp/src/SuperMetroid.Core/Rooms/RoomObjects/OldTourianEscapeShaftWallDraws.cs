using static SuperMetroid.Core.Rooms.RoomObjects.OldTourianEscapeShaftWallObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-wall draw of the old Tourian escape shaft: its family and block shape (#1862, #2034).</summary>
internal static class OldTourianEscapeShaftWallDraws
{
    /// <summary>The wall's draw: its opened blocks, pictured by the installed reveals and walls.</summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        RoomObjectDrawFamily<Draw>.Pictured<InstalledRoomObjectRevealAndWallVisuals>("reveal and wall",
            _ => OpenedWall, (visuals, draw) => visuals.Image(draw));
}
