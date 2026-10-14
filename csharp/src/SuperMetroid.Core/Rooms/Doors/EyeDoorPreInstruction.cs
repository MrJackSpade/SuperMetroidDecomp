namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Bank-$84 pre-instructions installed by both eye-door orientations.</summary>
internal enum EyeDoorPreInstruction : ushort
{
    /// <summary>Pre-instruction <c>$84:D753</c>, used by passive components awaiting the door bit.</summary>
    WakeWhenDoorBitSet = 0xd753,

    /// <summary>Pre-instruction <c>$84:BD50</c>, accepting missiles and Super Missiles.</summary>
    MissileHit = 0xbd50,
}
