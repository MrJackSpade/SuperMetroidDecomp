
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A part of an eye door and its facing.</summary>
/// <param name="Component">The component.</param>
/// <param name="Orientation">The orientation.</param>
internal readonly record struct EyeDoorPart(EyeDoorComponent Component, EyeDoorOrientation Orientation);
