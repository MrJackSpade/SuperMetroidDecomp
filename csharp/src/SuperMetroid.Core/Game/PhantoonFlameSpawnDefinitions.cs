namespace SuperMetroid.Core.Game;

/// <summary>
/// High byte of the destroyable-flame spawn parameter: the even jump-table offset
/// of the flame's initializer. The low byte is the type-specific index.
/// </summary>
public enum PhantoonFlameSpawnType : byte
{
    /// <summary>$00: a casual flame that falls from Phantoon and bounces.</summary>
    Casual = 0,
    /// <summary>$02: an enraged wave flame ($A7:D8C0/$D8D5), indexed by direction.</summary>
    Enraged = 2,
    /// <summary>$04: a rain flame, indexed by packed column and fall delay.</summary>
    Rain = 4,
    /// <summary>$06: a spiral flame, indexed by direction 0-7.</summary>
    Spiral = 6,
}

/// <summary>Authored Phantoon flame start angles and rain columns.</summary>
public static class PhantoonFlameSpawnDefinitions
{
    /// <summary>$86:98B4, PhantoonDestroyableFlameInit_Type2_Enraged. Eight clockwise angles followed by eight counterclockwise angles.</summary>
    public static byte RageAngle(int index) => (uint)index < 16
        ? (byte)(index < 8 ? (index + 1) * 16 : (23 - index) * 16)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>$86:98F7, PhantoonDestroyableFlameInit_Type4_Rain. Nine screen-X columns, 48 through 208, spaced twenty pixels apart.</summary>
    public static byte RainX(int column) => (uint)column < 9
        ? (byte)(48 + column * 20)
        : throw new ArgumentOutOfRangeException(nameof(column));

    /// <summary>$86:9979, PhantoonDestroyableFlameInit_Type6_Spiral. Eight equally spaced byte angles.</summary>
    public static byte SpiralAngle(int index) => (uint)index < 8
        ? (byte)(index * 32)
        : throw new ArgumentOutOfRangeException(nameof(index));
}
