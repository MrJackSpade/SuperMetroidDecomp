using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's glass block draws (#2034): the block shape each draws, from
/// <see cref="MotherBrainGlassObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class MotherBrainGlassDraws
{
    /// <summary>The glass's eleven frames, pictured by the installed glass frames.</summary>
    internal static RoomObjectDrawFamily<MotherBrainGlassDraw> Family { get; } =
        new RoomObjectDrawFamily<MotherBrainGlassDraw>("Mother Brain glass",
            frame => RoomObjectDrawFamily<MotherBrainGlassDraw>.Defined(MotherBrainGlassObjectDrawDefinitions.Shapes, frame));
}
