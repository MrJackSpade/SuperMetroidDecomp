namespace SuperMetroid.Core.Game;

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
