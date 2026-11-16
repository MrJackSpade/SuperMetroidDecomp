namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The elevator platform's three looks (#2127): the lights in its posts as they pulse, dim, glowing and bright. Its loop
/// shows them dim, glowing, bright, glowing (<see cref="ElevatorPlatformObjectDefinitions.Cycle"/>).
/// </summary>
internal enum ElevatorPlatformDraw
{
    /// <summary>The platform with its post lights dim, as each pulse starts.</summary>
    Dim,
    /// <summary>The platform with its post lights glowing, between dim and bright.</summary>
    Glowing,
    /// <summary>The platform with its post lights at their brightest.</summary>
    Bright,
}
