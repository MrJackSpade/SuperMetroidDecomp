namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One of the Alcoon's three fireball launches: the vertical speed its spit gives the fireball.</summary>
internal sealed class AlcoonFireballLaunch
{
    /// <summary>Creates the launch that sends the fireball off at <paramref name="yVelocity"/>.</summary>
    /// <param name="yVelocity">The fireball's vertical velocity at launch.</param>
    internal AlcoonFireballLaunch(SignedEightEight yVelocity) => YVelocity = yVelocity;

    /// <summary>The fireball's vertical velocity at launch, in pixels per frame.</summary>
    internal SignedEightEight YVelocity { get; }
}
