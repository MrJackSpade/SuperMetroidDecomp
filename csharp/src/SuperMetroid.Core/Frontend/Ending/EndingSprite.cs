using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A live ending actor with its role and role-specific motion: a picture actor (<see cref="EndingPictureSprite"/>) or a typed
/// text (<see cref="EndingTextSprite"/>), both running animations that publish the escape scenes' cues (#2195, #2197).
/// </summary>
/// <param name="role">What the actor shows and how it moves.</param>
internal abstract class EndingSprite(EndingSpriteRole role)
{
    /// <summary>What the actor shows and how it moves.</summary>
    public EndingSpriteRole Role { get; } = role;

    /// <summary>The actor: where it stands, whether it is alive, and its animation's next update.</summary>
    public abstract ICinematicActor<EndingSceneCue> Sprite { get; }

    /// <summary>The explosion stars' flyaway motion, or null for an actor the flyaway does not move.</summary>
    public EndingStarsMotion? StarsMotion { get; set; }

    /// <summary>Whether cloud motion has entered its moving cue phase.</summary>
    public bool CloudMoving { get; set; }

    /// <summary>
    /// The starfield's speed across the screen once the flyaway starts it: a quarter pixel an update to the right,
    /// slowing by 1/2048 of a pixel each update until the starfield leaves with the finale.
    /// </summary>
    public SignedSixteenSixteen FlyawayVelocity { get; set; }

    /// <summary>
    /// Draws what the actor shows from <paramref name="art"/> without advancing it: a picture from its family's
    /// presentation, or a typed text's letters.
    /// </summary>
    internal abstract void Draw(ScreenSprites screenSprites, EndingSpriteArt art);
}
