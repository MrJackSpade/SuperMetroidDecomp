using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a ninja pirate's thrown claw keeps: the way it was thrown, its speed, and whether it is still flying out or coming
/// back.
/// </summary>
/// <param name="launchSpeed">The claw's launch speed.</param>
/// <param name="thrown">The way the claw is thrown: it flies out that way and comes back the other.</param>
internal sealed class NinjaPirateClawState(ReturningClawSpeed launchSpeed, HorizontalDirection thrown) : EnemyProjectileState
{
    /// <summary>The way the claw is thrown, which its pirate chose: it flies out that way and comes back the other.</summary>
    internal HorizontalDirection Thrown { get; } = thrown;

    /// <summary>
    /// The claw's speed along its throw (#2284): it slows to rest while the claw flies out, a whole number of updates from
    /// its launch, and speeds up once the claw comes back. Only its whole pixels move the claw (<see cref="StepAlongThrow"/>).
    /// </summary>
    internal ReturningClawSpeed Speed { get; private set; } = launchSpeed;

    /// <summary>Whether the claw is still flying out; it turns back once its outward speed has run out.</summary>
    internal bool Outbound { get; private set; } = true;

    /// <summary>
    /// One update of the claw's flight: the whole pixels it moves along its throw, positive while it flies out and negative
    /// once it comes back, from the speed before this update's change. The speed then slows while flying out, turning the
    /// claw back on the update it comes to rest, or speeds up while coming back.
    /// </summary>
    internal int StepAlongThrow()
    {
        int pixels = Speed.WholePixels;
        if (!Outbound)
        {
            Speed = Speed.SpedUpComingBack();
            return -pixels;
        }
        Speed = Speed.SlowedFlyingOut();
        if (Speed.IsAtRest)
            Outbound = false;
        return pixels;
    }
}
