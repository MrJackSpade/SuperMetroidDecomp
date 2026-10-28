namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A spread bomb's horizontal travel: a direction and an unsigned speed of 0 through 127 whole pixels plus a fraction
/// in 1/256-pixel steps per frame. A wall reverses the direction and keeps the speed.
/// </summary>
public readonly record struct BombSpreadHorizontalVelocity
{
    /// <summary>Stores already range-checked parts.</summary>
    private BombSpreadHorizontalVelocity(bool movingLeft, int wholePixels, int fractionSteps)
    {
        MovingLeft = movingLeft;
        WholePixels = wholePixels;
        FractionSteps = fractionSteps;
    }

    /// <summary>Whether the bomb travels left; otherwise it travels right.</summary>
    public bool MovingLeft { get; }

    /// <summary>Whole pixels travelled per frame, 0 through 127.</summary>
    public int WholePixels { get; }

    /// <summary>Additional 1/256 pixels travelled per frame, 0 through 255.</summary>
    public int FractionSteps { get; }

    /// <summary>A launch moving left or right at <paramref name="wholePixels"/> and <paramref name="fractionSteps"/> 256ths of a pixel per frame.</summary>
    internal static BombSpreadHorizontalVelocity Of(bool movingLeft, int wholePixels, int fractionSteps) =>
        new(movingLeft, wholePixels, fractionSteps);

    /// <summary>The same speed in the opposite direction, as a wall collision leaves it.</summary>
    public BombSpreadHorizontalVelocity Reversed() => new(!MovingLeft, WholePixels, FractionSteps);

    /// <summary>The per-frame displacement as a signed 16.16 value: leftward travel is negative.</summary>
    public SignedSixteenSixteen Displacement
    {
        get
        {
            SignedSixteenSixteen speed = SignedSixteenSixteen.FromParts(
                checked((short)WholePixels), checked((ushort)(FractionSteps * 256)));
            return MovingLeft ? SignedSixteenSixteen.Zero - speed : speed;
        }
    }
}
