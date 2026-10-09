namespace SuperMetroid.Core.Game;

/// <summary>
/// The five live PPU Mode 7 words consumed by <c>Samus_CalcPos_Mode7</c> at
/// <c>$8B:8A52</c> while Ceres status has its high bit set.
/// </summary>
/// <param name="MatrixA">Signed 8.8 coefficient used for both diagonal terms by the cartridge routine.</param>
/// <param name="MatrixB">Signed 8.8 coefficient applied to the vertical distance for the horizontal output.</param>
/// <param name="MatrixC">Signed 8.8 coefficient applied to the horizontal distance for the vertical output.</param>
/// <param name="CenterX">Horizontal world-coordinate origin used to form and restore the transformed point.</param>
/// <param name="CenterY">Vertical world-coordinate origin; the routine measures vertical distance upward from this value.</param>
/// <remarks>
/// This is deliberately a presentation value, not part of Samus's kinematics. Retail
/// temporarily replaces <c>$0AF6/$0AFA</c> with the transformed point, calculates the
/// spritemap origin, and restores both world coordinates immediately afterward. Keeping
/// the transform immutable makes it difficult for drawing code to accidentally rotate
/// collision, camera tracking, or the following gameplay frame's starting position.
/// Matrix D is absent because this particular cartridge routine uses A for both diagonal
/// terms; B and C provide the two cross terms exactly as written by bank <c>$8B</c>.
/// </remarks>
public readonly record struct SamusMode7Transform(
    ushort MatrixA,
    ushort MatrixB,
    ushort MatrixC,
    ushort CenterX,
    ushort CenterY)
{
    /// <summary>
    /// Applies the cartridge's signed 8.8 matrix math, including every intermediate
    /// 16-bit truncation that can be observed near a coordinate or arithmetic wrap.
    /// </summary>
    public SamusMode7Point Transform(ushort worldX, ushort worldY)
    {
        // `$8B:8A52` forms both distances with 16-bit subtraction before treating the
        // products as signed. In particular, vertical distance points upward: M7Y - Y.
        ushort horizontalDistance = unchecked((ushort)(worldX - CenterX));
        ushort verticalDistance = unchecked((ushort)(CenterY - worldY));

        // The original helper multiplies two signed 16-bit values into a 32-bit result.
        // Each product is shifted independently, truncated to a word, and only then added
        // to the other term. Do not combine these into a wider host expression: overflow
        // between the two `$26` additions is authentic 65816 behavior.
        ushort transformedHorizontalDistance = unchecked((ushort)(
            MultiplySigned8Point8(horizontalDistance, MatrixA) +
            MultiplySigned8Point8(MatrixB, verticalDistance)));
        ushort transformedVerticalDistance = unchecked((ushort)(
            MultiplySigned8Point8(MatrixC, horizontalDistance) +
            MultiplySigned8Point8(MatrixA, verticalDistance)));

        return new SamusMode7Point(
            unchecked((ushort)(CenterX + transformedHorizontalDistance)),
            unchecked((ushort)(CenterY - transformedVerticalDistance)));
    }

    /// <summary>Multiplies signed 8.8 words and returns the product shifted right eight bits with 16-bit truncation.</summary>
    /// <param name="left">First coefficient or distance, interpreted as a signed 16-bit fixed-point value.</param>
    /// <param name="right">Second coefficient or distance, interpreted as a signed 16-bit fixed-point value.</param>
    /// <returns>The low 16 bits of the signed product after the cartridge's arithmetic shift.</returns>
    private static ushort MultiplySigned8Point8(ushort left, ushort right)
    {
        int product = unchecked((short)left) * unchecked((short)right);
        return unchecked((ushort)(product >> 8));
    }
}

/// <summary>A transformed temporary world point returned by <see cref="SamusMode7Transform"/>.</summary>
/// <param name="X">Horizontal coordinate after applying the temporary Mode 7 transform.</param>
/// <param name="Y">Vertical coordinate after applying the temporary Mode 7 transform.</param>
public readonly record struct SamusMode7Point(ushort X, ushort Y);
