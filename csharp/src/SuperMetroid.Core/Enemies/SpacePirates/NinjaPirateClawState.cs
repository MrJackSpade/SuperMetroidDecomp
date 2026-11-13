using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a ninja pirate's thrown claw keeps: its speed, and whether it is still flying out or coming back.</summary>
/// <param name="speed">The claw's launch speed.</param>
internal sealed class NinjaPirateClawState(SignedEightEight speed) : EnemyProjectileState
{
    /// <summary>How much a claw's speed changes each update: 32 256ths of a pixel an update.</summary>
    private static readonly SignedEightEight SpeedChange = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>
    /// The claw's sideways speed, a wrapping speed: it slows while the claw flies out and speeds up once it comes back,
    /// continuing past the largest speed as the game's addition does. Only its whole pixels, read unsigned, move the claw
    /// (<see cref="StepAlongThrow"/>), so a returning claw that keeps flying long enough speeds up to 255 pixels an update
    /// and then starts again from rest.
    /// </summary>
    internal SignedEightEight Speed { get; set; } = speed;

    /// <summary>Whether the claw is still flying out; it turns back once its outward speed has run out.</summary>
    internal bool Outbound { get; set; } = true;

    /// <summary>
    /// One update of the claw's flight: the whole pixels it moves along its throw, positive while it flies out and negative
    /// once it comes back, from the speed before this update's change. The speed then slows while flying out, turning the
    /// claw back on the update it reaches zero, or speeds up while coming back.
    /// </summary>
    internal int StepAlongThrow()
    {
        int pixels = Speed.UnsignedWholePixels;
        bool outbound = Outbound;
        Speed = outbound ? Speed - SpeedChange : Speed + SpeedChange;
        if (outbound && Speed.IsZero)
            Outbound = false;
        return outbound ? pixels : -pixels;
    }
}
