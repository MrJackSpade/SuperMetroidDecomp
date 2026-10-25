using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The gradual fade of a set of colours toward target colours ($82:DA02/$82:DA4A/$82:DAA6)
/// that death, doors, maps and several bosses share.
/// </summary>
internal sealed class GradualPaletteTransition
{
    /// <summary>Native divisor controlling how far each interpolation call moves.</summary>
    private readonly int denominator;
    /// <summary>Shared WRAM fade counter advanced and reset by this transition.</summary>
    private readonly GradualColorChangeCounter numerator;

    /// <summary>Captures the destination palette and binds the native shared fade counter.</summary>
    /// <param name="target">The colours to fade to, an image of the colours the transition changes.</param>
    /// <param name="denominator">Native interpolation divisor, one through 65534; transition number denominator plus one installs the exact target.</param>
    /// <param name="numerator">The shared <c>$7E:C400</c> transition number this fade advances.</param>
    public GradualPaletteTransition(ColourImage target, int denominator, GradualColorChangeCounter numerator)
    {
        Ensure.NotNull(numerator);
        Ensure.NotNull(target);
        if (denominator is <= 0 or > (ushort.MaxValue - 1))
            throw new ArgumentOutOfRangeException(nameof(denominator));
        Target = target;
        this.denominator = denominator;
        this.numerator = numerator;
    }

    /// <summary>
    /// Advances one native call. The first call (transition number zero) is intentionally a
    /// no-op, number denominator+1 reaches the exact target, and the following call returns
    /// complete while resetting the native counter.
    /// </summary>
    public bool Step()
    {
        int transitionNumber = numerator.Numerator;
        if (transitionNumber > denominator + 1)
        {
            numerator.Reset();
            return true;
        }

        for (int run = 0; run < Target.RunCount; run++)
        {
            ColourRun colours = Target.Run(run);
            ReadOnlySpan<RgbColor> goals = Target.Copy(run);
            for (int index = 0; index < colours.Count; index++)
            {
                RgbColor current = colours[index];
                if (current != goals[index])
                    colours.Write(index, CalculateColor(transitionNumber, current, goals[index]));
            }
        }
        numerator.Advance();
        return false;
    }

    /// <summary>The colours the transition fades to, which a caller may change without resetting fade progress or current colours.</summary>
    internal ColourImage Target { get; }

    /// <summary>Applies native fixed-point interpolation independently to each RGB5 channel.</summary>
    private RgbColor CalculateColor(int step, RgbColor current, RgbColor destination) =>
        current.Zip(destination, (_, from, to) => CalculateComponent(step, from, to));

    /// <summary>Moves one channel toward its target using the remaining-step divisor and 8.8 truncation.</summary>
    private int CalculateComponent(int step, int current, int destination)
    {
        if (step == 0)
            return current;
        int zeroBasedStep = step - 1;
        if (zeroBasedStep == denominator)
            return destination;

        // An 8.8 fixed-point step: the channel distance in 256ths per remaining step.
        int fixedDelta = Math.Abs(destination - current) * 256 /
                         (denominator - zeroBasedStep);
        if (destination < current)
            fixedDelta = -fixedDelta;
        return EightEight.WholeOf(EightEight.FromWhole(current) + fixedDelta);
    }
}
