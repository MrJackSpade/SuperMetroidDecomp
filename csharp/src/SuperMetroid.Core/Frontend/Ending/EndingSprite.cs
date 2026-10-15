using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Associates a live cinematic actor with its native slot and role-specific motion.</summary>
internal sealed record EndingSprite(IntroDiscoverySprite Sprite, EndingSpriteRole Role)
{
    /// <summary>Physical slot used by descending native actor traversal.</summary>
    public int NativeSlot { get; init; }
    /// <summary>Whether cloud motion has entered its moving instruction phase.</summary>
    public bool CloudMoving { get; set; }
}
