using static SuperMetroid.Core.Rooms.RoomObjects.CrateriaMainstreetEscapePassageObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-passage draw of Crateria's main street escape: its family and block shape (#1862, #2034).</summary>
internal static class CrateriaMainstreetEscapePassageDraws
{
    /// <summary>The passage's draw: its two open blocks, pictured by the installed reveals and walls.</summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        RoomObjectDrawFamily<Draw>.Pictured<InstalledRoomObjectRevealAndWallVisuals>("reveal and wall",
            _ => OpenPassage, (visuals, draw) => visuals.Image(draw));
}
