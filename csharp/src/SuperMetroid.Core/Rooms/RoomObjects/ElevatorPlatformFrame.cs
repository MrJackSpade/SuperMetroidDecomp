namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One frame of an elevator platform's loop (#2553): the layout it draws at the platform's block and how many updates it
/// stays. The loop only draws, so its frames hold no move and no sound.
/// </summary>
/// <param name="Draw">What the frame draws at the platform's block.</param>
/// <param name="Updates">The updates the frame stays before the next frame draws.</param>
internal readonly record struct ElevatorPlatformFrame(RoomObjectDraw Draw, int Updates);
