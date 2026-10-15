namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The five live PPU Mode 7 words consumed by <c>Samus_CalcPos_Mode7</c> at
/// <c>$8B:8A52</c> while Ceres status has its high bit set.
/// </summary>
/// <remarks>
/// This is deliberately a presentation value, not part of Samus's kinematics. Retail
/// temporarily replaces <c>$0AF6/$0AFA</c> with the transformed point, calculates the
/// spritemap origin, and restores both world coordinates immediately afterward. Keeping
/// the transform immutable makes it difficult for drawing code to accidentally rotate
/// collision, camera tracking, or the following gameplay frame's starting position.
/// Matrix D is absent because this particular cartridge routine uses A for both diagonal
/// terms; B and C provide the two cross terms exactly as written by bank <c>$8B</c>.
/// </remarks>
/// <param name="MatrixA">M7A, also used for M7D.</param>
/// <param name="MatrixB">M7B.</param>
/// <param name="MatrixC">M7C.</param>
/// <param name="Center">M7X/M7Y: the room point the shaft's layer-one Mode 7 plane rotates about.</param>
public readonly record struct SamusMode7Transform(
    ushort MatrixA,
    ushort MatrixB,
    ushort MatrixC,
    Position16<RoomPixels> Center)
{
    /// <summary>
    /// Applies the cartridge's signed 8.8 matrix math, including every intermediate
    /// 16-bit truncation that can be observed near a coordinate or arithmetic wrap.
    /// </summary>
    public Position16<RoomPixels> Transform(Position16<RoomPixels> world)
    {
        // `$8B:8A52` forms both distances with 16-bit subtraction before treating the
        // products as signed. In particular, vertical distance points upward: M7Y - Y.
        ushort horizontalDistance = unchecked((ushort)(world.X - Center.X));
        ushort verticalDistance = unchecked((ushort)(Center.Y - world.Y));

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

        return new Position16<RoomPixels>(
            unchecked((ushort)(Center.X + transformedHorizontalDistance)),
            unchecked((ushort)(Center.Y - transformedVerticalDistance)));
    }

    /// <summary>Multiplies two signed matrix/distance words and truncates the shifted result to the cartridge's 16-bit intermediate.</summary>
    private static ushort MultiplySigned8Point8(ushort left, ushort right)
    {
        int product = unchecked((short)left) * unchecked((short)right);
        return unchecked((ushort)SnesEightEight.WholeOf(product));
    }
}
