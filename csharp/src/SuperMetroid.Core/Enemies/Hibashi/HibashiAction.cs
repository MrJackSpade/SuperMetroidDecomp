namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The behaviour Hibashi's main AI runs. The flame switches between one inactive timer and one
/// animation-driven active eruption.
/// Its values are arbitrary.
/// </summary>
public enum HibashiAction
{
    /// <summary>Waits out the flame's rest, then starts the eruption animation again and lets its unseen hitbox part touch Samus.</summary>
    Inactive,
    /// <summary>Lets the eruption animation's cues show the flame and size its hitbox until it finishes, then rests again for its spawn's wait.</summary>
    Active,
}
