using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a ninja pirate's thrown claw keeps: its speed, and whether it is still flying out or coming back.</summary>
/// <param name="launchSpeed">The claw's launch speed.</param>
internal sealed class NinjaPirateClawState(SignedEightEight launchSpeed) : EnemyProjectileState
{
    /// <summary>How much a claw flying out slows each update, in 256ths of a pixel.</summary>
    private const int OutwardDeceleration = 32;

    /// <summary>
    /// The claw's sideways speed in 256ths of a pixel per update, never negative: it slows to rest while the claw flies
    /// out, a whole number of updates from its launch, and speeds up once the claw comes back
    /// (<see cref="ReturningClawSpeed"/>). Only its whole pixels move the claw (<see cref="StepAlongThrow"/>).
    /// </summary>
    internal int Speed { get; private set; } = launchSpeed.UnsignedTwoHundredFiftySixthsOfPixel;

    /// <summary>Whether the claw is still flying out; it turns back once its outward speed has run out.</summary>
    internal bool Outbound { get; private set; } = true;

    /// <summary>
    /// One update of the claw's flight: the whole pixels it moves along its throw, positive while it flies out and negative
    /// once it comes back, from the speed before this update's change. The speed then slows while flying out, turning the
    /// claw back on the update it reaches zero, or speeds up while coming back.
    /// </summary>
    internal int StepAlongThrow()
    {
        int pixels = Speed / 256;
        if (!Outbound)
        {
            Speed = ReturningClawSpeed.Accelerated(Speed);
            return -pixels;
        }
        Speed -= OutwardDeceleration;
        if (Speed < 0)
            throw new InvalidOperationException($"A claw launched at {launchSpeed} passed rest without stopping.");
        if (Speed == 0)
            Outbound = false;
        return pixels;
    }
}
