namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A bomb spread bomb's flight (#2459), created when the spread places the bomb and dropped when the bomb ends: its
/// horizontal velocity, which a wall reverses, its vertical velocity, which Samus's gravity accelerates every update,
/// and the launch velocity a floor bounce restores. Velocities are in pixels per update, negative leftward and upward.
/// </summary>
public sealed class BombSpreadFlight
{
    /// <summary>Starts a flight launched with <paramref name="horizontalVelocity"/> and <paramref name="launchVelocity"/>.</summary>
    internal BombSpreadFlight(SignedSixteenSixteen horizontalVelocity, SignedSixteenSixteen launchVelocity)
    {
        HorizontalVelocity = horizontalVelocity;
        LaunchVelocity = launchVelocity;
        VerticalVelocity = launchVelocity;
    }

    /// <summary>The bomb's horizontal velocity, negative leftward; a wall reverses it.</summary>
    public SignedSixteenSixteen HorizontalVelocity { get; private set; }

    /// <summary>
    /// The bomb's vertical velocity, negative upward: the launch velocity at first, accelerated by Samus's gravity every
    /// update, restored by a floor bounce and stopped by a ceiling (<see cref="StopRiseAtCeiling"/>).
    /// </summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>The vertical velocity the bomb was launched with, its held-Down bonus included, which a floor bounce restores.</summary>
    private SignedSixteenSixteen LaunchVelocity { get; }

    /// <summary>Accelerates the bomb's fall by <paramref name="gravity"/>, the fraction carrying into the whole pixels.</summary>
    internal void Accelerate(SignedSixteenSixteen gravity) => VerticalVelocity += gravity;

    /// <summary>Turns the bomb back from a wall: its horizontal velocity reversed, whole and fraction together.</summary>
    internal void ReverseAtWall() => HorizontalVelocity = -HorizontalVelocity;

    /// <summary>Bounces the bomb off a floor: it rises again at its launch velocity.</summary>
    internal void BounceOffFloor() => VerticalVelocity = LaunchVelocity;

    /// <summary>
    /// Stops the bomb's rise at a ceiling, keeping the fraction of a pixel its speed holds as a slow fall (#2459): a
    /// rise of a pixel and a quarter an update leaves a fall of three quarters of a pixel an update; a rise of whole
    /// pixels leaves the bomb at rest.
    /// </summary>
    internal void StopRiseAtCeiling() => VerticalVelocity = VerticalVelocity.WithoutWholePixels();
}
