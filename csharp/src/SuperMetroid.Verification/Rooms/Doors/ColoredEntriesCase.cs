using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The color and orientation of one colored entries case.</summary>
/// <param name="Color">The color.</param>
/// <param name="Orientation">The orientation.</param>
internal readonly record struct ColoredEntriesCase(ColoredDoorColor Color, ColoredDoorOrientation Orientation);
