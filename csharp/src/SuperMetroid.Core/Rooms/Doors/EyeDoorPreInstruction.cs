namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The pre-instructions both eye-door orientations install. The values are arbitrary (#1414).</summary>
internal enum EyeDoorPreInstruction
{
    /// <summary>Wakes a passive component once the door bit is set.</summary>
    WakeWhenDoorBitSet = 1,

    /// <summary>Wakes the eye on a missile or Super Missile hit.</summary>
    MissileHit = 2,
}
