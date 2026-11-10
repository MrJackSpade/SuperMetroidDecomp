namespace SuperMetroid.Core.Earthquakes;

/// <summary>
/// What an earthquake shakes: the room's foreground layer, its background layer, and the enemies with their projectiles
/// (<see cref="EarthquakeShakeDefinitions"/>). Every earthquake shakes at least one of the two layers; the enemies shake
/// only with the background.
/// </summary>
public enum EarthquakeReach
{
    /// <summary>The foreground layer alone.</summary>
    Foreground,

    /// <summary>Both layers; the enemies stay still.</summary>
    ForegroundAndBackground,

    /// <summary>Both layers, the enemies and their projectiles.</summary>
    ForegroundBackgroundAndEnemies,

    /// <summary>The background layer, the enemies and their projectiles; the foreground stays still.</summary>
    BackgroundAndEnemies,
}
