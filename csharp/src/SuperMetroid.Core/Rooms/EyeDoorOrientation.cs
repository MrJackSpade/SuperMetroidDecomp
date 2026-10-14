namespace SuperMetroid.Core.Rooms;

/// <summary>Mirrored horizontal orientation of an eye-door component.</summary>
public enum EyeDoorOrientation : byte
{
    /// <summary>Left-facing native component set $84:DB56/$DB5A/$DB60, selecting mirrored artwork, projectile operands, and the left-facing blue-door replacement.</summary>
    Left,
    /// <summary>Right-facing native component set $84:DB48/$DB4C/$DB52, selecting mirrored artwork, projectile operands, and the right-facing blue-door replacement.</summary>
    Right,
}
