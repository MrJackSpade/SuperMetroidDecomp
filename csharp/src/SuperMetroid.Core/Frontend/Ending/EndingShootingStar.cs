using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One post-credits star: its own flight's definition, and its position, velocity, start delay and pictures.</summary>
internal struct EndingShootingStar
{
    /// <summary>How the star flies: its acceleration, its pictures' pace and its start delay (#2541).</summary>
    internal EndingShootingStarDefinition Definition;
    /// <summary>
    /// The updates of its authored start delay the star still waits, counted down to none, after which its next update
    /// starts it (#2488); null once it flies.
    /// </summary>
    internal int? DelayLeft;
    /// <summary>The star's pictures through its flight (<see cref="EndingShootingStarAnimations"/>).</summary>
    internal AnimationPlayer<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue> Flight;
    /// <summary>
    /// Signed screen place, to a fraction of a pixel, before centering the sprite: left of or above the screen is negative, so a star
    /// leaving by the top or left edge is tested against <see cref="EndingShootingStarDefinitions.SpriteOffset"/> as it is.
    /// </summary>
    internal FixedVector<ScreenPixels> Position;
    /// <summary>The star's velocity, which its acceleration builds up.</summary>
    internal EightEightVector<ScreenPixels> Velocity;

    /// <summary>What the star shows.</summary>
    internal readonly EndingShootingStarLook Look => Flight.Showing;

    /// <summary>Whether the star is still waiting out its authored start delay.</summary>
    internal readonly bool Delayed => DelayLeft is not null;
}
