using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Domain-owned transitions over <see cref="ColoredDoorOrientation"/>.</summary>
public static class ColoredDoorOrientations
{
    /// <summary>The orientation at <paramref name="index"/> of a four-entry left/right/up/down door table.</summary>
    public static ColoredDoorOrientation AtTableIndex(int index) => index switch
    {
        0 => ColoredDoorOrientation.Left,
        1 => ColoredDoorOrientation.Right,
        2 => ColoredDoorOrientation.Up,
        3 => ColoredDoorOrientation.Down,
        _ => throw new InvalidOperationException($"Door orientation table index {index} has no {nameof(ColoredDoorOrientation)}."),
    };
}
