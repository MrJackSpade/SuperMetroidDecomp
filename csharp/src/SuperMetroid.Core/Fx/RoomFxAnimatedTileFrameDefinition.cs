namespace SuperMetroid.Core.Fx;

/// <summary>One timed frame of a room-FX animated-tile object's loop.</summary>
/// <param name="Duration">How many passes the frame is shown for.</param>
public readonly record struct RoomFxAnimatedTileFrameDefinition(int Duration)
{
}
