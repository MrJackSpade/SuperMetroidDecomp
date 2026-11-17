namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How far a door transition pushes Samus on each step of the door-opening scroll: a door's own speed, or the game's
/// default for the door's axis.
/// </summary>
public readonly record struct DoorSamusSpeed
{
    /// <summary>The finest part of a pixel a push holds: a 256th.</summary>
    private const int FinestParts = 256;

    /// <summary>The default push through a vertical door: one and a half pixels per step.</summary>
    private static readonly SignedEightEight VerticalDefault = Pixels(1, 1, 2);

    /// <summary>The default push through a horizontal door: twenty-five thirty-seconds of a pixel per step.</summary>
    private static readonly SignedEightEight HorizontalDefault = Pixels(0, 25, 32);

    /// <summary>The door's own speed, or <see langword="null"/> for the axis default.</summary>
    private readonly SignedEightEight? _perStep;

    /// <summary>Keeps <paramref name="perStep"/>.</summary>
    private DoorSamusSpeed(SignedEightEight? perStep) => _perStep = perStep;

    /// <summary>The game's default push for the door's axis; most doors use it.</summary>
    public static DoorSamusSpeed AxisDefault { get; } = new(null);

    /// <summary>No push: Samus keeps her position through the scroll, as elevator doors leave her.</summary>
    public static DoorSamusSpeed None { get; } = new(SignedEightEight.Zero);

    /// <summary>
    /// A door's own push of <paramref name="wholePixels"/> and <paramref name="numerator"/>/<paramref name="denominator"/>
    /// of a pixel per step (#2356): a forward push, so it is not negative.
    /// </summary>
    /// <param name="wholePixels">The push's whole pixels, 0 through 127.</param>
    /// <param name="numerator">The parts of a pixel past the whole pixels, less than <paramref name="denominator"/>.</param>
    /// <param name="denominator">The parts of a pixel the push's fraction counts, a divisor of 256.</param>
    /// <exception cref="ArgumentOutOfRangeException">A part of the push lies outside its range.</exception>
    public static DoorSamusSpeed PixelsPerStep(int wholePixels, int numerator = 0, int denominator = 1) =>
        new(Pixels(wholePixels, numerator, denominator));

    /// <summary>A door's own push of <paramref name="perStep"/> per step: a forward push, so it is not negative.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="perStep"/> is negative.</exception>
    public static DoorSamusSpeed PerStep(SignedEightEight perStep) =>
        perStep.IsNegative
            ? throw new ArgumentOutOfRangeException(nameof(perStep), perStep, "A door pushes Samus forward.")
            : new(perStep);

    /// <summary>The push per step through a door travelled toward <paramref name="direction"/>, as a forward speed.</summary>
    public SignedEightEight Step(DoorDirection direction) =>
        _perStep ?? (direction.IsVertical() ? VerticalDefault : HorizontalDefault);

    /// <summary>A forward speed of <paramref name="wholePixels"/> and <paramref name="numerator"/>/<paramref name="denominator"/> of a pixel.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A part of the speed lies outside its range.</exception>
    private static SignedEightEight Pixels(int wholePixels, int numerator, int denominator)
    {
        int parts = Ensure.BetweenInclusive(1, FinestParts, denominator);
        if (FinestParts % parts != 0)
            throw new ArgumentOutOfRangeException(nameof(denominator), denominator, "A push's fraction counts a divisor of 256 parts.");
        return SignedEightEight.FromParts(Ensure.AtLeastZero(wholePixels),
            Ensure.BetweenInclusive(0, parts - 1, numerator) * (FinestParts / parts));
    }
}
