using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>The colour-math registers one composition uses: CGADSUB's sources and operation, CGWSEL's subscreen-or-fixed operand select, and COLDATA's five-bit fixed colour (#633).</summary>
public readonly record struct ColorMathRegisters
{
    /// <summary>CGADSUB source and operation bits.</summary>
    public required SnesColorMathControl ColorMath { get; init; }

    /// <summary>Whether arithmetic uses the winning subscreen pixel instead of fixed color.</summary>
    public required bool AddSubscreen { get; init; }

    /// <summary>Five-bit fixed-color red component.</summary>
    public required byte FixedRed { get; init; }

    /// <summary>Five-bit fixed-color green component.</summary>
    public required byte FixedGreen { get; init; }

    /// <summary>Five-bit fixed-color blue component.</summary>
    public required byte FixedBlue { get; init; }
}
