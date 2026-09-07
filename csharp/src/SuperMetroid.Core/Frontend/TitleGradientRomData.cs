namespace SuperMetroid.Core.Frontend;

/// <summary>Native title gradient HDMA definitions, selected by the Mode-7 zoom register.</summary>
public static class TitleGradientRomData
{
    /// <summary>Bank $8C containing the title fixed-color HDMA streams.</summary>
    public const int FixedColorBank = 0x8c0000;
    /// <summary>$8C:BC5D, TitleSequenceHDMATables: sixteen pointers indexed by zoom bits 4..7.</summary>
    public const int FixedColorPointers = 0x8cbc5d;
    /// <summary>$88:EB95, HDMATable_ColorMathControlRegB_TitleSequenceGradient: CGADSUB runs.</summary>
    public const int ControlTable = 0x88eb95;
}
