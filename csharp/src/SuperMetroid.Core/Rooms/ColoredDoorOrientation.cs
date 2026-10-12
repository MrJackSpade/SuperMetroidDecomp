using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Door-cap orientation shared by colored and blue BTS tables.</summary>
public enum ColoredDoorOrientation : byte
{
    /// <summary>Left-facing cap; orientation index 0 corresponds to blue-door BTS $40.</summary>
    Left,
    /// <summary>Right-facing cap; orientation index 1 corresponds to blue-door BTS $41.</summary>
    Right,
    /// <summary>Upward-facing cap; orientation index 2 corresponds to blue-door BTS $42.</summary>
    Up,
    /// <summary>Downward-facing cap; orientation index 3 corresponds to blue-door BTS $43.</summary>
    Down,
}
