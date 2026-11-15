using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room block draws (#2034): the wall fill, the escape door, the twelve mural rows, the two
/// solid rows and the six tube clears, the block shape each draws, from <see cref="MotherBrainFakeDeathObjectDrawDefinitions"/>,
/// and the installed picture (#1557) each shows.
/// </summary>
internal static class MotherBrainFakeDeathDraws
{
    /// <summary>The fake-death room's frames, pictured by the installed fake-death room frames.</summary>
    internal static RoomObjectDrawFamily<MotherBrainFakeDeathDraw> Family { get; } =
        RoomObjectDrawFamily<MotherBrainFakeDeathDraw>.Pictured<InstalledMotherBrainFakeDeathFrames>("Mother Brain fake-death room",
            frame => RoomObjectDrawFamily<MotherBrainFakeDeathDraw>.Defined(MotherBrainFakeDeathObjectDrawDefinitions.Shapes, frame),
            (frames, draw) => frames.Images.Image(draw.Frame));
}
