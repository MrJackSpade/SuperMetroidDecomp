namespace SuperMetroid.Core.Game;

/// <summary>Fixed entry timers for Kraid's private head instruction programs.</summary>
internal static class KraidHeadTimers
{
    /// <summary>$A7:96D2, InstList_Kraid_Roar_0 timer.</summary>
    public const ushort Roar = 10;
    /// <summary>$A7:974A, InstList_Kraid_EyeGlowing_0 timer; the initializer ticks it immediately.</summary>
    public const ushort EyeGlow = 5;
    /// <summary>$A7:9764, InstList_Kraid_Dying_0 timer.</summary>
    public const ushort Death = 25;
    /// <summary>$A7:96EC (selector 26) holds the open mouth for 64 frames; 96F4/96FC/9704 (34/42/50) each hold for 10.</summary>
    public static ushort GrowthResume(int byteSelector) => byteSelector == 26 ? (ushort)64 : Roar;
}
