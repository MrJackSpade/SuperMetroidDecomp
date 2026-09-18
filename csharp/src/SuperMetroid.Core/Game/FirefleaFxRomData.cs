namespace SuperMetroid.Core.Game;

/// <summary>Definition data for FX $24 initialization ($88:B07C) and pre-instruction ($88:B0BC).</summary>
public static class FirefleaFxRomData
{
    /// <summary>$88:B058, Fireflea_Flashing_Shades: twelve packed shade words.</summary>
    public const int FlashingShades = 0x88B058;
    /// <summary>$88:B070, Fireflea_Darkness_Shades, indexed by the enemy-owned byte offset.</summary>
    public const int DarknessShades = 0x88B070;
    /// <summary>$7E:1778, FirefleaFlashing_Timer, decremented only while time is not frozen.</summary>
    public const int Timer = 0x1778;
    /// <summary>$7E:177A, FirefleaFlashing_Index, a word-sized flashing-table element index.</summary>
    public const int Index = 0x177A;
    /// <summary>$7E:177E, FirefleaFlashing_DarknessLevel, advanced by dying Firefleas.</summary>
    public const int Darkness = 0x177E;
}
