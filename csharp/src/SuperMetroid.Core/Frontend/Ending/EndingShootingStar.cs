using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Mutable position, velocity, start delay and pictures of one post-credits star.</summary>
internal struct EndingShootingStar
{
    /// <summary>Whether the star is still waiting out its authored start delay, counted by <see cref="DelayLeft"/>.</summary>
    internal bool Delayed;
    /// <summary>The updates of its start delay the star still waits after this one, while <see cref="Delayed"/>.</summary>
    internal int DelayLeft;
    /// <summary>The star's pictures through its flight (<see cref="EndingShootingStarAnimations"/>).</summary>
    internal AnimationPlayer<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue> Flight;
    /// <summary>
    /// Fixed-point signed screen place before centering the sprite: left of or above the screen is negative, so a star
    /// leaving by the top or left edge is tested against <see cref="EndingShootingStarDefinitions.SpriteOffset"/> as it is.
    /// </summary>
    internal FixedVector<ScreenPixels> Position;
    /// <summary>Current signed 1/256-pixel motion accumulated by the star's acceleration.</summary>
    internal EightEightVector<ScreenPixels> Velocity;

    /// <summary>What the star shows.</summary>
    internal readonly EndingShootingStarLook Look => Flight.Showing;
}
