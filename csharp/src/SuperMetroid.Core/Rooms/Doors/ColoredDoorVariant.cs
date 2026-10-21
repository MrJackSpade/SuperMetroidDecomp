
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A coloured door's colour and facing.</summary>
/// <param name="Color">The color.</param>
/// <param name="Orientation">The orientation.</param>
internal readonly record struct ColoredDoorVariant(ColoredDoorColor Color, ColoredDoorOrientation Orientation);
