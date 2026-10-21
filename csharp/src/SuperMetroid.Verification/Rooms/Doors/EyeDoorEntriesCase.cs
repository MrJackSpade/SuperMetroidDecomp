using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The orientation and component of one eye door entries case.</summary>
/// <param name="Orientation">The orientation.</param>
/// <param name="Component">The component.</param>
internal readonly record struct EyeDoorEntriesCase(EyeDoorOrientation Orientation, EyeDoorComponent Component);
