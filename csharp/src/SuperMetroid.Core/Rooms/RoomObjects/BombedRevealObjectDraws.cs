using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectBombedRevealDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The bombed reveals: their family, each one block colliding as its definition says (#1862, #2034).</summary>
internal static partial class BombedRevealObjectDraws
{
    /// <summary>The three reveals, pictured by the installed reveals and walls.</summary>
    internal static RoomObjectDrawFamily<RoomObjectBombedRevealDraw> Family { get; } =
        new RoomObjectDrawFamily<RoomObjectBombedRevealDraw>("reveal and wall",
            frame => RoomBlockShapes.Block(Collisions.TryGetValue(frame, out var collision)
                ? collision
                : throw new InvalidOperationException($"Undefined bombed-reveal draw {frame}.")));
}
