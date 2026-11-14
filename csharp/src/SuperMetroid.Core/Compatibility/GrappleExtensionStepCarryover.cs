namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The extending grapple rope's step keeps parts of the update before's step (#1752). Each update the rope's end moves
/// in <see cref="Substeps"/> equal substeps, probing for a block or enemy after each, and each substep on each axis is a
/// quarter of the rope's extension speed; but the game builds the quarter over the update before's step without clearing
/// it, so the substeps need not add up to the speed. On every axis the step's fraction gains a quarter of the part of the
/// update before's fraction finer than the speed's 256ths of a pixel. Along an axis the rope extends rightward or
/// downward, the step's whole pixels also gain a quarter of the update before's whole pixels counted in whole 256-pixel
/// lengths, and an odd fraction the update before sets the step back half the room's coordinate range, which carries the
/// end across the coordinate seam; along an axis it extends leftward or upward, the step keeps only the speed's own whole
/// pixels.
/// </summary>
/// <remarks>
/// A rule with no state: the step it builds is kept by the rope (<c>SamusGrappleState</c>), cleared when the beam is
/// fired, and handed back the next update. Consulted once an update while the beam extends, after the firing update's
/// enemy test and before its substeps move the end (<c>SamusGrappleMovement.StepFiring</c>). Evidence: the original
/// extension routine divides the speed's fraction and whole parts into the storage that held the update before's quarter
/// with partial stores, leaving the parts the speed does not reach; the grapple extension arithmetic check
/// (<c>GrappleExtensionArithmeticVerification</c>) compares the step for every speed over previous steps that cover each
/// kept part with the arithmetic it replaced.
/// </remarks>
[CompatibilityService("the extending grapple rope's step keeps parts of the update before's step, so its four substeps need not add up to its speed")]
public static class GrappleExtensionStepCarryover
{
    /// <summary>The substeps the extending rope moves and probes in each update, each by a quarter of its speed.</summary>
    public const int Substeps = 4;

    /// <summary>The whole pixels of an extension speed's cycle, a speed's whole pixels continuing from zero past it.</summary>
    private const int SpeedWholePixelCycle = 256;

    /// <summary>How far an odd fraction the update before sets the step back: half the room's coordinate range, in pixels.</summary>
    private const int OddFractionSetBack = CoordinateAxis.LastNonNegative + 1;

    /// <summary>One substep's share of the speed, in 256ths: the factor that scales the speed to its quarter.</summary>
    private static readonly int SubstepShareInTwoHundredFiftySixths = EightEight.FromWhole(1) / Substeps;

    /// <summary>The parts of a pixel's fraction in each 256th of a pixel, the finest step an extension speed takes.</summary>
    private static readonly int FractionPartsPerSpeedStep = SixteenSixteen.FromEightEight(1);

    /// <summary>
    /// One substep of the rope's extension on one axis: a quarter of <paramref name="speed"/>, rounded toward negative
    /// infinity, with the parts of the update before's step <paramref name="previous"/> the speed does not reach kept in it.
    /// </summary>
    /// <param name="speed">The rope's extension speed on the axis.</param>
    /// <param name="previous">The substep the rope moved by on the axis the update before.</param>
    public static SignedSixteenSixteen Step(SignedEightEight speed, SignedSixteenSixteen previous)
    {
        SignedSixteenSixteen quarter = speed.ScaledBy(SubstepShareInTwoHundredFiftySixths);
        int whole = quarter.WholePixels;
        if (!speed.IsNegative)
        {
            int keptWholeLengths = previous.UnsignedWholePixels / SpeedWholePixelCycle * SpeedWholePixelCycle;
            whole += keptWholeLengths / Substeps;
            if (previous.Fraction % 2 != 0)
                whole -= OddFractionSetBack;
        }

        int fraction = quarter.Fraction + previous.Fraction % FractionPartsPerSpeedStep / Substeps;
        return SignedSixteenSixteen.FromParts(whole, fraction);
    }
}
