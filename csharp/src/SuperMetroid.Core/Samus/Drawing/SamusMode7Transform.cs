namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The Ceres elevator shaft's rotation: how far the shaft's turning floor has rotated Samus's surroundings about its
/// centre, which <c>Samus_CalcPos_Mode7</c> at <c>$8B:8A52</c> applies to her drawing while Ceres status has its high
/// bit set.
/// </summary>
/// <remarks>
/// This is deliberately a presentation value, not part of Samus's kinematics. Retail
/// temporarily replaces <c>$0AF6/$0AFA</c> with the transformed point, calculates the
/// spritemap origin, and restores both world coordinates immediately afterward. Keeping
/// the transform immutable makes it difficult for drawing code to accidentally rotate
/// collision, camera tracking, or the following gameplay frame's starting position.
/// The second diagonal term is absent because this particular cartridge routine uses A for both diagonal
/// terms; B and C provide the two cross terms exactly as written by bank <c>$8B</c>.
/// </remarks>
/// <param name="MatrixA">The rotation's step through room X per room X, also its step through room Y per room Y: a signed 8.8 fixed-point coefficient (256 is 1.0).</param>
/// <param name="MatrixB">The rotation's step through room X per room Y: a signed 8.8 fixed-point coefficient.</param>
/// <param name="MatrixC">The rotation's step through room Y per room X: a signed 8.8 fixed-point coefficient.</param>
/// <param name="Center">The room point the shaft's turning floor rotates about.</param>
/// <remarks>
/// The coefficients are signed sixteen-bit 8.8 values: every product below treats them as signed and truncates
/// to sixteen bits, so their width is part of the transform, and they are stored signed exactly as they are
/// consumed.
/// </remarks>
public readonly record struct SamusMode7Transform(
    short MatrixA,
    short MatrixB,
    short MatrixC,
    Position16<RoomPixels> Center)
{
    /// <summary>
    /// Applies the cartridge's signed 8.8 matrix math, including every intermediate
    /// 16-bit truncation that can be observed near a coordinate or arithmetic wrap.
    /// </summary>
    public Position16<RoomPixels> Transform(Position16<RoomPixels> world)
    {
        // `$8B:8A52` forms both distances with 16-bit subtraction before treating the
        // products as signed. In particular, vertical distance points upward: centre Y - Y.
        ushort horizontalDistance = unchecked((ushort)(world.X - Center.X));
        ushort verticalDistance = unchecked((ushort)(Center.Y - world.Y));

        // The original helper multiplies two signed 16-bit values into a 32-bit result.
        // Each product is shifted independently, truncated to a word, and only then added
        // to the other term. Do not combine these into a wider host expression: overflow
        // between the two `$26` additions is authentic 65816 behavior.
        ushort transformedHorizontalDistance = unchecked((ushort)(
            MultiplySigned8Point8(horizontalDistance, MatrixA) +
            MultiplySigned8Point8(verticalDistance, MatrixB)));
        ushort transformedVerticalDistance = unchecked((ushort)(
            MultiplySigned8Point8(horizontalDistance, MatrixC) +
            MultiplySigned8Point8(verticalDistance, MatrixA)));

        return new Position16<RoomPixels>(
            unchecked((ushort)(Center.X + transformedHorizontalDistance)),
            unchecked((ushort)(Center.Y - transformedVerticalDistance)));
    }

    /// <summary>Multiplies a distance word, read as signed, by a signed coefficient and truncates the shifted result to the cartridge's 16-bit intermediate.</summary>
    private static ushort MultiplySigned8Point8(ushort distance, short coefficient)
    {
        int product = unchecked((short)distance) * coefficient;
        return unchecked((ushort)SnesEightEight.WholeOf(product));
    }
}
