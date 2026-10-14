namespace SuperMetroid.Core.Game;

/// <summary>Instruction-list entry points selected directly by palette-FX callbacks.</summary>
public static class PaletteFxInstructionListPointers
{
    /// <summary>Power-suit Norfair heat list at $8D:E45E.</summary>
    public const ushort NorfairPowerSuit = 0xe45e;
    /// <summary>Varia-suit Norfair heat list at $8D:E68A.</summary>
    public const ushort NorfairVariaSuit = 0xe68a;
    /// <summary>Gravity-suit Norfair heat list at $8D:E8B6.</summary>
    public const ushort NorfairGravitySuit = 0xe8b6;
    /// <summary>First vertical-switch continuation list at $8D:EB43.</summary>
    public const ushort AboveY380 = 0xeb43;
    /// <summary>Second vertical-switch continuation list at $8D:EC76.</summary>
    public const ushort AboveY380Second = 0xec76;
}
