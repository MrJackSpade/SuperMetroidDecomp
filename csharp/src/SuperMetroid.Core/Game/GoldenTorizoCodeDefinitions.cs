namespace SuperMetroid.Core.Game;

/// <summary>Literal inventory writes in InitAI_Torizo.GTCode at $AA:C917-C95A.</summary>
internal static class GoldenTorizoCodeDefinitions
{
    /// <summary>$AA:C919 CMP operand: exactly held A+B+X+Y, with no other controller bits.</summary>
    internal const ushort ControllerChord = 0xc0c0;
    /// <summary>$AA:C91E immediate: current and maximum energy, exactly 700 rather than 799.</summary>
    internal const ushort Energy = 700;
    /// <summary>$AA:C927 immediate: current and maximum reserve energy; reserve mode is not written.</summary>
    internal const ushort ReserveEnergy = 300;
    /// <summary>$AA:C930 immediate: current and maximum missiles.</summary>
    internal const ushort Missiles = 100;
    /// <summary>$AA:C939 immediate: current and maximum Super Missiles.</summary>
    internal const ushort SuperMissiles = 20;
    /// <summary>$AA:C942 immediate: current and maximum Power Bombs.</summary>
    internal const ushort PowerBombs = 20;
    /// <summary>$AA:C94B immediate: replaces collected and equipped item words without merging prior upgrades.</summary>
    internal const ushort Items = 0xf337;
    /// <summary>$AA:C954 immediate: native collected/equipped beams, including simultaneous Spazer and Plasma.</summary>
    internal const ushort Beams = 0x100f;
}
