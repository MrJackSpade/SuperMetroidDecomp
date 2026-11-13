using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Associates a live cinematic actor with its role and role-specific motion.</summary>
internal sealed record EndingSprite(IntroDiscoverySprite Sprite, EndingSpriteRole Role)
{
    /// <summary>Whether cloud motion has entered its moving cue phase.</summary>
    public bool CloudMoving { get; set; }

    /// <summary>
    /// The starfield's speed across the screen once the flyaway starts it: a quarter pixel an update to the right,
    /// slowing by 1/2048 of a pixel each update until the starfield leaves with the finale.
    /// </summary>
    public SignedSixteenSixteen FlyawayVelocity { get; set; }
}
