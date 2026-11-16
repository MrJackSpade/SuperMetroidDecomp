using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>
/// Checks the Mama Turtle's wall reversal (#1620) against the original wall branch's negations and carries.
/// </summary>
internal static class MamaTurtleWallReversalVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every velocity whole word with each fraction class, and every acceleration whole word with each fraction class after
    /// a velocity at and either side of minus one pixel: the reversed velocity and acceleration equal the wall branch's
    /// negations, the acceleration taking the carry the velocity's addition leaves.
    /// </summary>
    internal static void VerifyMamaTurtleWallReversal()
    {
        int[] fractions = [0x0000, 0x0001, 0x1000, 0x8000, 0xffff];
        for (int velocityWhole = 0; velocityWhole < WordRange; velocityWhole++)
        foreach (int velocityFraction in fractions)
        {
            var velocity = SignedSixteenSixteen.FromParts(unchecked((short)velocityWhole), velocityFraction);
            int carried = velocityWhole + 1;
            var expectedVelocity = SignedSixteenSixteen.FromParts(
                unchecked((short)-(carried & ushort.MaxValue)), (-velocityFraction) & ushort.MaxValue);
            if (MamaTurtleWallReversal.ReversedVelocity(velocity) != expectedVelocity)
                throw new InvalidOperationException($"A turtle at {velocity} leaves the wall at the wrong velocity.");
        }

        // The acceleration reads only whether the velocity's whole pixels are minus one: a velocity at, and either side of, it.
        foreach (int velocityWhole in new[] { 0xfffe, 0xffff, 0x0000 })
        foreach (int velocityFraction in new[] { 0x0000, 0x8000 })
        {
            var velocity = SignedSixteenSixteen.FromParts(unchecked((short)velocityWhole), velocityFraction);
            int carry = velocityWhole + 1 > ushort.MaxValue ? 1 : 0;
            for (int accelerationWhole = 0; accelerationWhole < WordRange; accelerationWhole++)
            foreach (int accelerationFraction in fractions)
            {
                var acceleration = SignedSixteenSixteen.FromParts(unchecked((short)accelerationWhole), accelerationFraction);
                var expectedAcceleration = SignedSixteenSixteen.FromParts(
                    unchecked((short)~((accelerationWhole + carry) & ushort.MaxValue)), (-accelerationFraction) & ushort.MaxValue);
                if (MamaTurtleWallReversal.ReversedAcceleration(acceleration, velocity) != expectedAcceleration)
                    throw new InvalidOperationException($"A turtle at {velocity} accelerating {acceleration} leaves the wall at the wrong acceleration.");
            }
        }
        Console.WriteLine("  Mama Turtle wall reversal: every velocity and acceleration class matches the wall branch.");
    }
}
