using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's wall-cannon block draws (#2034): the block shape each draws, from
/// <see cref="DraygonCannonObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static partial class DraygonCannonDraws
{
    /// <summary>The cannon's frames, pictured by the installed cannon frames.</summary>
    internal static RoomObjectDrawFamily<DraygonCannonDraw> Family { get; } =
        new RoomObjectDrawFamily<DraygonCannonDraw>("Draygon cannon",
            frame => RoomObjectDrawFamily<DraygonCannonDraw>.Defined(DraygonCannonObjectDrawDefinitions.Shapes, frame));
}
