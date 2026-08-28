namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared fixed-point and angle primitives used by cartridge enemy families. These retain
/// the SNES's word wrapping and coarse integer division instead of substituting host floats.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports <c>CalculateAngleOf_12_14_Offset</c> at $A0:C0AF. Zero points upward and the
    /// result advances clockwise in 256 units per turn, matching bank-$A0 enemy callers.
    /// </summary>
    private static byte CalculateCartridgeAngle(short x, short y)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)x);
        ushort absoluteY = unchecked((ushort)y);
        if (x < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        if (absoluteY < absoluteX)
        {
            int divided = absoluteX == 0 ? 0 : (absoluteY << 8) / absoluteX;
            return quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + 64)),
                1 => unchecked((byte)(64 - (divided >> 3))),
                2 => unchecked((byte)(-64 - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - 64)),
            };
        }

        int inverseDivided = absoluteY == 0 ? 0 : (absoluteX << 8) / absoluteY;
        return quadrant switch
        {
            0 => unchecked((byte)(128 - (inverseDivided >> 3))),
            1 => unchecked((byte)(inverseDivided >> 3)),
            2 => unchecked((byte)((inverseDivided >> 3) + 128)),
            _ => unchecked((byte)(-(inverseDivided >> 3))),
        };
    }

    /// <summary>Adds one signed 8.8 velocity to a wrapped 16.16 position pair.</summary>
    private static (ushort Position, ushort Subposition) AddEightBitVelocity(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }
}
