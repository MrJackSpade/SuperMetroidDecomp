using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The gradual fade of a set of colours toward target colours
/// that death, doors, maps and several bosses share.
/// </summary>
internal sealed class GradualPaletteTransition
{
    /// <summary>Divisor controlling how far each interpolation call moves.</summary>
    private readonly int denominator;
    /// <summary>Shared fade counter advanced and reset by this transition.</summary>
    private readonly GradualColorChangeCounter numerator;

    /// <summary>Captures the destination palette and binds the shared fade counter.</summary>
    /// <param name="target">The colours to fade to, an image of the colours the transition changes.</param>
    /// <param name="denominator">Interpolation divisor, one through 65534; transition number denominator plus one installs the exact target.</param>
    /// <param name="numerator">The shared transition number this fade advances.</param>
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
    /// Advances one call. The first call (transition number zero) is intentionally a
    /// no-op, number denominator+1 reaches the exact target, and the following call returns
    /// complete while resetting the shared counter.
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

    /// <summary>Applies the shared gradual fade's step to each RGB5 channel (#1512).</summary>
    private RgbColor CalculateColor(int step, RgbColor current, RgbColor destination) =>
        GradualColourFade.Step(step, current, destination, denominator);
}
