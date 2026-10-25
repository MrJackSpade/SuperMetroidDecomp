namespace SuperMetroid.Core.Fx;

/// <summary>One timed frame of a room-FX animated-tile object's loop.</summary>
/// <param name="Object">The animated-tile object the frame belongs to.</param>
/// <param name="Index">The frame's place in the object's loop.</param>
/// <param name="Duration">How many passes the frame is shown for.</param>
public readonly record struct RoomFxAnimatedTileFrameDefinition(
    AnimatedTileObject Object,
    int Index,
    int Duration)
{
}
