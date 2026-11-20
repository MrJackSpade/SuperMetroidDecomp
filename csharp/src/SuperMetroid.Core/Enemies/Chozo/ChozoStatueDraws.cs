using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statue room objects' block draws (#2034): the two Wrecked Ship slope-access frames and Lower Norfair's
/// cleared hand, the block shape each draws, from <see cref="ChozoStatueObjectDrawDefinitions"/>, and the installed
/// picture (#1557) each shows.
/// </summary>
internal static class ChozoStatueDraws
{
    /// <summary>The statues' frames, pictured by the installed Chozo statue frames.</summary>
    internal static RoomObjectDrawFamily<ChozoStatueDraw> Family { get; } =
        new RoomObjectDrawFamily<ChozoStatueDraw>("Chozo statue",
            frame => RoomObjectDrawFamily<ChozoStatueDraw>.Defined(ChozoStatueObjectDrawDefinitions.Shapes, frame));
}
