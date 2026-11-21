using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One post-credits star: its own flight's definition, and its position, velocity, start delay and pictures.</summary>
internal partial struct EndingShootingStar
{
    /// <summary>How the star flies: its acceleration, its pictures' pace and its start delay (#2541).</summary>
    internal EndingShootingStarDefinition Definition;
    /// <summary>
    /// The star's authored start delay, counted once each update it waits; the update after it is over starts the star
    /// (#2488); null once it flies.
    /// </summary>
    internal UpdateCountdown? Delay;
    /// <summary>The star's pictures through its flight (<see cref="EndingShootingStarAnimations"/>).</summary>
    internal AnimationPlayer<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue> Flight;
    /// <summary>
    /// Signed screen place, to a fraction of a pixel, before centering the sprite: left of or above the screen is negative, so a star
    /// leaving by the top or left edge is tested against <see cref="EndingShootingStarDefinitions.SpriteOffset"/> as it is.
    /// </summary>
    internal FixedVector<ScreenPixels> Position;
    /// <summary>The star's velocity, which its acceleration builds up.</summary>
    internal PixelVelocity<ScreenPixels> Velocity;

    /// <summary>What the star shows.</summary>
    internal readonly EndingShootingStarLook Look => Flight.Showing;

    /// <summary>Whether the star is still waiting out its authored start delay.</summary>
    internal readonly bool Delayed => Delay is not null;
}
