namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks the pixel Samus's solid-enemy probe tests past her reach (#1621).</summary>
internal static class SolidEnemyProbeReachVerification
{
    private static readonly int[] Wholes = [0, 1, 2, 100, 0x7fff, 0x8000, 0xfffe, 0xffff];
    private static readonly int[] Fractions = [0, 1, 0x4000, 0x7fff, 0x8000, 0xc000, 0xffff];
    private static readonly int[] ReachWholes = [0, 1, 2, 15];

    /// <summary>
    /// Confirms <see cref="SolidEnemyProbeReach"/>: one pixel past a reach that ends inside a pixel, two past one whose
    /// fraction crosses a pixel's edge (an exact carry included), and pixel 0 itself when a crossing lands there, from
    /// both sides; then compares both directions with the subtract-or-add and second step it replaced.
    /// </summary>
    internal static void VerifySolidEnemyProbeReach()
    {
        AssertEqual(13, Forward(10, 0, 3, 0), "a whole reach right tests the reached pixel");
        AssertEqual(104, Forward(100, 0, 3, 0x4000), "a reach right ending inside a pixel tests one pixel further");
        AssertEqual(102, Forward(100, 0xf000, 0, 0x2000), "a fraction crossing right tests two pixels past the whole reach");
        AssertEqual(104, Forward(100, 0x9800, 2, 0x6800), "an exact carry to a whole pixel still tests one pixel further");
        AssertEqual(0, Forward(0xffff, 0xf000, 0, 0x2000), "a crossing right onto pixel 0 tests pixel 0 itself");
        AssertEqual(0, Forward(0xffff, 0x1000, 0, 0x2000), "a reach right inside the last pixel steps on to pixel 0");
        AssertEqual(8, Back(10, 0x8000, 2, 0x8000), "a whole reach left tests the reached pixel");
        AssertEqual(98, Back(100, 0, 0, 0x8000), "a fraction crossing left tests two pixels past the whole reach");
        AssertEqual(97, Back(100, 0, 1, 0x8000), "a crossing up tests two pixels past the whole reach");
        AssertEqual(0, Back(1, 0x4000, 0, 0x8000), "a crossing left onto pixel 0 tests pixel 0 itself");
        AssertEqual(0xfffe, Back(0, 0, 0, 0x8000), "a crossing left from pixel 0 wraps past the room's edge and steps again");
        AssertEqual(0xffff, Back(0, 0x8000, 0, 0x4000), "a reach left ending inside pixel 0 steps on past the room's edge");

        int compared = 0;
        foreach (int whole in Wholes)
        foreach (int fraction in Fractions)
        foreach (int reachWhole in ReachWholes)
        foreach (int reachFraction in Fractions)
        {
            AssertEqual(ReplacedForward(whole, fraction, reachWhole, reachFraction), Forward(whole, fraction, reachWhole, reachFraction),
                $"right from {whole:X4}.{fraction:X4} by {reachWhole}.{reachFraction:X4}");
            AssertEqual(ReplacedBack(whole, fraction, reachWhole, reachFraction), Back(whole, fraction, reachWhole, reachFraction),
                $"left from {whole:X4}.{fraction:X4} by {reachWhole}.{reachFraction:X4}");
            compared += 2;
        }

        Console.WriteLine($"Solid-enemy probe reach: outward steps, doubled crossings and the pixel-0 landing match; {compared} reaches agree with the replaced arithmetic.");
    }

    private static int Forward(int whole, int fraction, int reachWhole, int reachFraction) =>
        SolidEnemyProbeReach.TestedPixelForward(FixedCoordinate.Of(whole, fraction), new UnsignedSixteenSixteen(reachWhole, reachFraction));

    private static int Back(int whole, int fraction, int reachWhole, int reachFraction) =>
        SolidEnemyProbeReach.TestedPixelBack(FixedCoordinate.Of(whole, fraction), new UnsignedSixteenSixteen(reachWhole, reachFraction));

    /// <summary>The replaced rightward rounding: add with carry, then an increment, and a second one unless the first left zero.</summary>
    private static int ReplacedForward(int whole, int fraction, int reachWhole, int reachFraction)
    {
        int target = (whole + reachWhole) & 0xffff;
        int fractionSum = fraction + reachFraction;
        if (fractionSum > 0xffff)
        {
            target = (target + 1) & 0xffff;
            if (target != 0)
                target = (target + 1) & 0xffff;
        }
        else if (fractionSum != 0)
        {
            target = (target + 1) & 0xffff;
        }
        return target;
    }

    /// <summary>The replaced leftward rounding: subtract with borrow, then a decrement, and a second one unless the first left zero.</summary>
    private static int ReplacedBack(int whole, int fraction, int reachWhole, int reachFraction)
    {
        int target = (whole - reachWhole) & 0xffff;
        bool borrowed = fraction < reachFraction;
        int remainder = (fraction - reachFraction) & 0xffff;
        if (borrowed)
        {
            target = (target - 1) & 0xffff;
            if (target != 0)
                target = (target - 1) & 0xffff;
        }
        else if (remainder != 0)
        {
            target = (target - 1) & 0xffff;
        }
        return target;
    }
}
