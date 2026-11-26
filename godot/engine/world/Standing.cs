namespace Engine.World;

/// <summary>
/// Where something stands among what stands in a room, against the player: which of them is drawn over the other,
/// whenever each came into the room. Each is one of the room's depths (<see cref="RoomDepth.Of"/>); things of one
/// standing are drawn in the order they stand in the room. An enemy's data names its own ("standing"), written as
/// "in_front", "far_behind" and so on.
/// </summary>
public enum Standing
{
    /// <summary>Behind everything else standing in the room.</summary>
    Rearmost,

    /// <summary>Behind the things that are simply behind the player.</summary>
    FarBehind,

    /// <summary>Behind the player: where a thing stands unless it says otherwise.</summary>
    Behind,

    /// <summary>Behind the player and in front of everything else behind them.</summary>
    JustBehind,

    /// <summary>In front of the player.</summary>
    InFront,

    /// <summary>In front of the player and of everything else in front of them.</summary>
    Frontmost,

    /// <summary>
    /// Behind everything standing in the room, the rearmost of it too, whenever each came into the room: where
    /// something launched lies that the room's own things are all drawn over.
    /// </summary>
    Hindmost,

    /// <summary>
    /// In front of everything standing in the room, the frontmost of it too, whenever each came into the room: where
    /// something launched lies that is drawn over all the room's own things, which is where what is launched and
    /// left lies when nothing says otherwise.
    /// </summary>
    Foremost,
}
