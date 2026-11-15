using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian access floor's draws (<see cref="TourianAccessObjectDefinitions"/>, #2034): the block shape each opens,
/// from <see cref="TourianAccessObjectDrawDefinitions"/> (#1862), and the installed picture each shows.
/// </summary>
internal static class TourianAccessDraws
{
    /// <summary>The floor's frames, pictured by the installed Tourian access frames.</summary>
    internal static RoomObjectDrawFamily<TourianAccessDraw> Family { get; } =
        RoomObjectDrawFamily<TourianAccessDraw>.Pictured<RoomObjectTourianAccessVisuals>("Tourian access",
            frame => RoomObjectDrawFamily<TourianAccessDraw>.Defined(TourianAccessObjectDrawDefinitions.Shapes, frame),
            (visuals, draw) => visuals.Image(draw));
}
