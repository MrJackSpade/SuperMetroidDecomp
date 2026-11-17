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

    /// <summary>A crumbling row's breakup stages (#2291), pictured by the installed Tourian access frames.</summary>
    internal static RoomObjectDrawFamily<CrumbleDraw> CrumbleFamily { get; } =
        RoomObjectDrawFamily<CrumbleDraw>.Pictured<RoomObjectTourianAccessVisuals>("Tourian access crumble",
            frame => RoomObjectDrawFamily<BlockBreakupStage>.Defined(TourianAccessObjectDrawDefinitions.CrumbleShapes, frame.Stage),
            (visuals, draw) => visuals.Image(draw));

    /// <summary>A crumbling row showing <paramref name="stage"/> of its breakup.</summary>
    internal static RoomObjectFrameDraw<CrumbleDraw> Crumble(BlockBreakupStage stage) => CrumbleFamily.Of(new CrumbleDraw(stage));

    /// <summary>
    /// A crumbling row of the floor at one stage of its breakup, already open: the row's last stage is not drawn, as the
    /// empty row (<see cref="TourianAccessDraw.EmptyRow"/>) shows it gone.
    /// </summary>
    /// <param name="Stage">What the crumbling row shows.</param>
    internal readonly record struct CrumbleDraw(BlockBreakupStage Stage);
}
