namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's vertical velocity (#2569): how fast she moves vertically and which way, rising, falling or holding no
/// direction, as one value. Its signed reading (<see cref="Displacement"/>) is what moves her; its speed and direction stay
/// distinct at rest because the game's vertical motion tells them apart there: a rise slowed exactly to rest turns into a
/// fall from rest only on its next update, a fall from rest gathers speed at once, and a velocity with no direction holds
/// her still in the aerial moves that test for one. A rise that gravity slows past rest wraps to a speed that reads as
/// having passed its apex (<see cref="HasPassedApex"/>, <see cref="SamusSpeedMagnitude.HasPassedZero"/>).
/// </summary>
public readonly record struct SamusVerticalVelocity
{
    /// <summary>The velocity of <paramref name="speed"/> heading <paramref name="direction"/>.</summary>
    internal SamusVerticalVelocity(UnsignedSixteenSixteen speed, SamusVerticalDirection direction)
    {
        Speed = speed;
        Direction = direction switch
        {
            SamusVerticalDirection.None or SamusVerticalDirection.Up or SamusVerticalDirection.Down => direction,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Undefined {nameof(SamusVerticalDirection)}."),
        };
    }

    /// <summary>How fast she moves vertically, whichever way: the speed gravity changes and the landing reads.</summary>
    public UnsignedSixteenSixteen Speed { get; }

    /// <summary>Which way the velocity carries her: up, down, or no direction at all.</summary>
    public SamusVerticalDirection Direction { get; }

    /// <summary>No vertical motion and no direction, as standing on the ground or a pose that clears her motion leaves her.</summary>
    public static SamusVerticalVelocity Still => default;

    /// <summary>A fall from rest: no speed yet, heading down.</summary>
    public static SamusVerticalVelocity FallFromRest { get; } = new(UnsignedSixteenSixteen.Zero, SamusVerticalDirection.Down);

    /// <summary>A rise at <paramref name="speed"/>.</summary>
    public static SamusVerticalVelocity Rising(UnsignedSixteenSixteen speed) => new(speed, SamusVerticalDirection.Up);

    /// <summary>A fall at <paramref name="speed"/>.</summary>
    public static SamusVerticalVelocity Falling(UnsignedSixteenSixteen speed) => new(speed, SamusVerticalDirection.Down);

    /// <summary>
    /// The velocity <paramref name="velocity"/> gives, downward positive: a fall for a downward one, a rise for an upward
    /// one, and at rest heading <paramref name="atRest"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="atRest"/> is neither up nor down.</exception>
    public static SamusVerticalVelocity FromDisplacement(SignedSixteenSixteen velocity, VerticalDirection atRest)
    {
        VerticalDirection heading = velocity.IsPositive ? VerticalDirection.Down : velocity.IsNegative ? VerticalDirection.Up : atRest;
        return heading switch
        {
            VerticalDirection.Down => Falling(velocity.Magnitude()),
            VerticalDirection.Up => Rising(velocity.Magnitude()),
            _ => throw new ArgumentOutOfRangeException(nameof(atRest), atRest, "Not a vertical direction."),
        };
    }

    /// <summary>Whether the velocity heads up.</summary>
    public bool IsRising => Direction == SamusVerticalDirection.Up;

    /// <summary>Whether the velocity heads down.</summary>
    public bool IsFalling => Direction == SamusVerticalDirection.Down;

    /// <summary>Whether the velocity has no direction, as on the ground.</summary>
    public bool HasNoDirection => Direction == SamusVerticalDirection.None;

    /// <summary>Whether she moves vertically at any speed, whichever way the velocity heads.</summary>
    public bool IsMoving => !Speed.IsZero;

    /// <summary>
    /// Whether the velocity is a rise that gravity has slowed past rest: its speed wrapped to one that reads as having
    /// passed zero (<see cref="SamusSpeedMagnitude.HasPassedZero"/>), the apex a jump, bounce or released swing turns at.
    /// </summary>
    public bool HasPassedApex => IsRising && SamusSpeedMagnitude.HasPassedZero(Speed.Whole);

    /// <summary>
    /// How far the velocity moves her in one update, downward positive: the speed downward for a fall, and upward for a rise
    /// or a velocity with no direction, as the vertical move takes every velocity that does not fall.
    /// </summary>
    public SignedSixteenSixteen Displacement => Direction switch
    {
        SamusVerticalDirection.Down => Speed.AsDisplacement(),
        SamusVerticalDirection.Up or SamusVerticalDirection.None => -Speed.AsDisplacement(),
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusVerticalDirection)} {Direction}."),
    };

    /// <summary>The velocity with its speed replaced by <paramref name="speed"/>, its direction kept.</summary>
    public SamusVerticalVelocity WithSpeed(UnsignedSixteenSixteen speed) => new(speed, Direction);

    /// <summary>The velocity brought to rest, its direction kept: a rise stopped this way still turns at its apex.</summary>
    public SamusVerticalVelocity Stopped() => WithSpeed(UnsignedSixteenSixteen.Zero);

    /// <summary>The velocity turned to fall at its own speed, whichever way it headed.</summary>
    public SamusVerticalVelocity TurnedDownward() => Falling(Speed);

    /// <summary>
    /// The velocity after one update's gravity of <paramref name="acceleration"/>: a fall gathers it unless its whole pixels
    /// stand exactly at <paramref name="terminalWholePixels"/>, where it holds, and a rise, or a velocity with no direction,
    /// loses it, a rise slowed past rest wrapping to a speed past its apex.
    /// </summary>
    /// <param name="acceleration">The gravity of her medium.</param>
    /// <param name="terminalWholePixels">The whole pixels of the fastest fall the gravity leaves alone.</param>
    public SamusVerticalVelocity AfterGravity(UnsignedSixteenSixteen acceleration, int terminalWholePixels) => IsFalling
        ? Speed.Whole != terminalWholePixels ? WithSpeed(Speed + acceleration) : this
        : WithSpeed(Speed - acceleration);
}
