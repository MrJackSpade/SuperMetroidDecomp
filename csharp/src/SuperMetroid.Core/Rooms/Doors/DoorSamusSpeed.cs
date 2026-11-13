namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How far a door transition pushes Samus on each step of the door-opening scroll: a door's own speed, or the game's
/// default for the door's axis.
/// </summary>
public readonly record struct DoorSamusSpeed
{
    /// <summary>The default push through a vertical door: one and a half pixels per step.</summary>
    private static readonly SignedEightEight VerticalDefault = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(384);

    /// <summary>The default push through a horizontal door: 200/256 pixel per step.</summary>
    private static readonly SignedEightEight HorizontalDefault = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(200);

    /// <summary>The door's own speed, or <see langword="null"/> for the axis default.</summary>
    private readonly SignedEightEight? _perStep;

    /// <summary>Keeps <paramref name="perStep"/>.</summary>
    private DoorSamusSpeed(SignedEightEight? perStep) => _perStep = perStep;

    /// <summary>The game's default push for the door's axis; most doors use it.</summary>
    public static DoorSamusSpeed AxisDefault { get; } = new(null);

    /// <summary>No push: Samus keeps her position through the scroll, as elevator doors leave her.</summary>
    public static DoorSamusSpeed None { get; } = new(SignedEightEight.Zero);

    /// <summary>
    /// A door's own push of <paramref name="subpixels"/> 1/256-pixel units per step: a forward push, so it is not negative,
    /// and a speed, so it lies within a signed 8.8 speed's range.
    /// </summary>
    public static DoorSamusSpeed SubpixelsPerStep(int subpixels) =>
        new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Ensure.AtLeastZero(subpixels)));

    /// <summary>The push per step through a door travelled toward <paramref name="direction"/>, as a forward speed.</summary>
    public SignedEightEight Step(DoorDirection direction) =>
        _perStep ?? (direction.IsVertical() ? VerticalDefault : HorizontalDefault);
}
