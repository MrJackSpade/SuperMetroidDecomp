namespace SuperMetroid.Core.Rendering;

/// <summary>The colour-math registers one composition uses: CGADSUB's sources and operation, CGWSEL's subscreen-or-fixed operand select, and COLDATA's five-bit fixed colour (#633).</summary>
public readonly record struct ColorMathRegisters
{
    /// <summary>CGADSUB source and operation bits.</summary>
    public required SnesColorMathControl ColorMath { get; init; }

    /// <summary>Whether arithmetic uses the winning subscreen pixel instead of fixed color.</summary>
    public required bool AddSubscreen { get; init; }

    /// <summary>Fixed-color red intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public required int FixedRed { get; init; }

    /// <summary>Fixed-color green intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public required int FixedGreen { get; init; }

    /// <summary>Fixed-color blue intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public required int FixedBlue { get; init; }
}
