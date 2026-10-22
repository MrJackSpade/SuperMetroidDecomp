namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The object character sheets the ending scenes draw their actors from.</summary>
internal enum EndingObjectSheet
{
    /// <summary>The escape scenes' cloud sheet, used until the planet explosion begins.</summary>
    Clouds,
    /// <summary>The explosion and flyaway sheet, used from the planet explosion onward.</summary>
    Explosion,
    /// <summary>The post-credits reward sheet.</summary>
    Reward,
    /// <summary>
    /// The shooting scene's sheet, which the jumping Samus switches to once she passes above the screen and over which
    /// the logo later assembles.
    /// </summary>
    RewardFalling,
}
