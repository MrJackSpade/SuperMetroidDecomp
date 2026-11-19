namespace SuperMetroid.Core.Assets;

/// <summary>
/// Which of the gameplay scene's layers a screen shows (#1407, #1514): any combination of the room's foreground, its
/// background, a boss's body and the actors. An ordinary room shows them all; a door transition shows the actors, with or
/// without the foreground; the waterfall rooms show their background only through the waterfall's blend; a boss hides its
/// body as its fight says.
/// </summary>
/// <remarks>The members combine freely; their values are arbitrary.</remarks>
[Flags]
public enum ScreenLayers
{
    /// <summary>No layer and no actors.</summary>
    None = 0,
    /// <summary>The room's foreground, its back tiles and its front tiles.</summary>
    Foreground = 1 << 0,
    /// <summary>The room's background, its back tiles and its front tiles.</summary>
    Background = 1 << 1,
    /// <summary>The actors: Samus, enemies, projectiles and the other drawn objects, at each of their depths.</summary>
    Sprites = 1 << 2,
    /// <summary>A boss's body, its own layer (#2573), at each of its parts' depths.</summary>
    BossBody = 1 << 3,
}
