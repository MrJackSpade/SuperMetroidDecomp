namespace SuperMetroid.Core.Game;

/// <summary>Compiled collision geometry, independent of Kraid's editable head artwork.</summary>
internal static class KraidMouthHitboxes
{
    /// <summary>Aligned fixed records at $A7:9788..97C7; other pointers retain address-space semantics.</summary>
    public static bool IsDefined(ushort pointer) => pointer >= 0x9788 && pointer <= 0x97c0 && ((pointer - 0x9788) & 7) == 0;

    /// <summary>
    /// $A7:9788..97C7, Hitbox_KraidMouth_0..7. The native projectile test uses
    /// left/top/bottom only; the authored right edge is retained as definition data.
    /// Entry four is unused but remains a defined all-zero rectangle.
    /// </summary>
    public static (short Left, short Top, short Right, short Bottom) Resolve(ushort pointer) => pointer switch
    {
        0x9788 => (16, -120, 40, -88),
        0x9790 => (16, -120, 40, -104),
        0x9798 => (16, -128, 40, -112),
        0x97a0 => (16, -128, 40, -120),
        0x97a8 => (0, 0, 0, 0),
        0x97b0 => (6, -96, 32, -80),
        0x97b8 => (0, -104, 32, -80),
        0x97c0 => (0, -112, 32, -80),
        _ => throw new InvalidDataException($"Undefined Kraid mouth hitbox $A7:{pointer:X4}."),
    };
}
