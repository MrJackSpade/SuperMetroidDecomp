using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>
/// Checks the grapple's firing arithmetic (#1632) against the shift and bit constants it replaced: the rope's quarter
/// extension step for every velocity over previous steps that cover each leaked part, the endpoint's block for every
/// room coordinate, and the octant angle for every displacement in a wide square and along the axes' extremes.
/// </summary>
internal static class GrappleExtensionArithmeticVerification
{
    /// <summary>Compares the quarter step, the endpoint block and the octant angle with the replaced arithmetic.</summary>
    internal static void VerifyGrappleExtensionArithmetic()
    {
        int[] previousWholes = [0, 1, 255, 256, 257, 1023, 16384, short.MaxValue, short.MinValue, -1, -256, -257];
        int[] previousFractions = [0, 1, 2, 3, 63, 64, 255, 256, 257, 32768, ushort.MaxValue - 1, ushort.MaxValue];
        int quarterCompared = 0, quarterMismatches = 0;
        string? firstQuarterMismatch = null;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            SignedEightEight velocity = SignedEightEight.Wrapped(raw);
            foreach (int previousWhole in previousWholes)
            {
                foreach (int previousFraction in previousFractions)
                {
                    SignedSixteenSixteen previous = SignedSixteenSixteen.FromParts(previousWhole, previousFraction);
                    SignedSixteenSixteen expected = ReplacedQuarterExtensionVelocity(velocity, previous);
                    SignedSixteenSixteen actual = GrappleExtensionStepCarryover.Step(velocity, previous);
                    quarterCompared++;
                    if (actual == expected)
                        continue;
                    quarterMismatches++;
                    firstQuarterMismatch ??= $"velocity {raw}, previous {previousWhole}:{previousFraction}: expected {expected.RawValue}, got {actual.RawValue}";
                }
            }
        }
        AssertEqual(0, quarterMismatches,
            $"every quarter extension step equals the replaced constants' ({quarterCompared} compared; first mismatch: {firstQuarterMismatch})");

        int blockMismatches = 0;
        for (int pixel = 0; pixel <= ushort.MaxValue; pixel++)
        {
            Position16<RoomBlocks> block = new Position16<RoomPixels>(pixel, ushort.MaxValue - pixel).ToRoomBlock();
            blockMismatches += block.X == CoordinateAxis.Wrap(pixel >> 4) && block.Y == CoordinateAxis.Wrap((ushort.MaxValue - pixel) >> 4) ? 0 : 1;
        }
        AssertEqual(0, blockMismatches, "every endpoint coordinate's block is the coordinate shifted right four");

        int angleCompared = 0, angleMismatches = 0;
        string? firstAngleMismatch = null;
        void CompareAngle(int x, int y)
        {
            var delta = new Offset16<RoomPixels>(x, y);
            RotationAngle expected = ReplacedCalculateAngleFromXY(delta);
            RotationAngle actual = SamusGrappleMovement.CalculateAngleFromXY(delta);
            angleCompared++;
            if (actual == expected)
                return;
            angleMismatches++;
            firstAngleMismatch ??= $"({x},{y}): expected {expected}, got {actual}";
        }

        for (int y = -600; y <= 600; y++)
        {
            for (int x = -600; x <= 600; x++)
                CompareAngle(x, y);
        }
        foreach (int far in new[] { short.MinValue, short.MinValue + 1, -20000, 20000, short.MaxValue - 1, short.MaxValue })
        {
            for (int near = -600; near <= 600; near++)
            {
                CompareAngle(far, near);
                CompareAngle(near, far);
            }
            foreach (int otherFar in new[] { short.MinValue, -20000, 20000, short.MaxValue })
                CompareAngle(far, otherFar);
        }
        AssertEqual(0, angleMismatches,
            $"every displacement's octant angle equals the replaced shift's ({angleCompared} compared; first mismatch: {firstAngleMismatch})");

        Console.WriteLine(
            $"Grapple extension arithmetic: {quarterCompared} quarter steps, every endpoint block and {angleCompared} octant angles equal the replaced constants'.");
    }

    /// <summary>The quarter extension step as it was written before #1632, with its shift constants as literals.</summary>
    private static SignedSixteenSixteen ReplacedQuarterExtensionVelocity(SignedEightEight velocity, SignedSixteenSixteen previous)
    {
        SignedSixteenSixteen quarter = velocity.ScaledBy(64);
        int whole = quarter.WholePixels;
        if (!velocity.IsNegative)
        {
            whole += previous.UnsignedWholePixels / 256 * 64;
            if (previous.Fraction % 2 != 0)
                whole -= 32768;
        }

        int fraction = quarter.Fraction + previous.Fraction % 256 / 4;
        return SignedSixteenSixteen.FromParts(whole, fraction);
    }

    /// <summary>The octant angle as it was written before #1632, reading each tangent's steps by a shift of three.</summary>
    private static RotationAngle ReplacedCalculateAngleFromXY(Offset16<RoomPixels> delta)
    {
        int x = delta.X;
        int y = delta.Y;
        int absoluteX = Math.Abs(x);
        int absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return RotationAngle.Zero;

        int quadrant = (x < 0 ? 2 : 0) | (y < 0 ? 1 : 0);
        if (absoluteY < absoluteX)
        {
            int eighths = (EightEight.FromWhole(absoluteY) / absoluteX) >> 3;
            return quadrant switch
            {
                0 => RotationAngle.QuarterTurn.TurnedBySteps(eighths),
                1 => RotationAngle.QuarterTurn.TurnedBySteps(-eighths),
                2 => RotationAngle.ThreeQuarterTurn.TurnedBySteps(-eighths),
                _ => RotationAngle.ThreeQuarterTurn.TurnedBySteps(eighths),
            };
        }

        int reciprocalEighths = (EightEight.FromWhole(absoluteX) / absoluteY) >> 3;
        return quadrant switch
        {
            0 => RotationAngle.HalfTurn.TurnedBySteps(-reciprocalEighths),
            1 => RotationAngle.Zero.TurnedBySteps(reciprocalEighths),
            2 => RotationAngle.HalfTurn.TurnedBySteps(reciprocalEighths),
            _ => RotationAngle.Zero.TurnedBySteps(-reciprocalEighths),
        };
    }
}
