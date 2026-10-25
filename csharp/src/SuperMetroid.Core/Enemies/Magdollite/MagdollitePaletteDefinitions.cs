namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The shape of Magdollite's colour cycle, not its timing.</summary>
public static class MagdollitePaletteDefinitions
{
    /// <summary>The cycle steps through all four authored colour rows.</summary>
    public const int FrameCount = 4;

    /// <summary>Each step replaces the colours from the tenth (index nine) of Magdollite's colour set.</summary>
    public const int FirstAnimatedColor = 9;

    /// <summary>The four colours each step replaces, nine through twelve; the other twelve stay unchanged.</summary>
    public const int AnimatedColorCount = 4;
}
