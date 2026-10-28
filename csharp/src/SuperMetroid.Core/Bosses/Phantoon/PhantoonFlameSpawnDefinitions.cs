namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored Phantoon flame start angles and rain columns.</summary>
public static class PhantoonFlameSpawnDefinitions
{
    /// <summary>$86:98B4, PhantoonDestroyableFlameInit_Type2_Enraged. Eight clockwise angles followed by eight counterclockwise angles.</summary>
    internal static readonly byte[] RageAngles = [16, 32, 48, 64, 80, 96, 112, 128, 240, 224, 208, 192, 176, 160, 144, 128];

    /// <summary>$86:98F7, PhantoonDestroyableFlameInit_Type4_Rain. Nine screen-X columns, 48 through 208, spaced twenty pixels apart.</summary>
    internal static readonly byte[] RainXs = [48, 68, 88, 108, 128, 148, 168, 188, 208];

    /// <summary>$86:9979, PhantoonDestroyableFlameInit_Type6_Spiral. Eight equally spaced byte angles.</summary>
    internal static readonly byte[] SpiralAngles = [0, 32, 64, 96, 128, 160, 192, 224];
}
