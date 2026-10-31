namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The authored waits after a game load: the updates the game holds a black screen, running no phase, before the
/// gameplay fade begins. How long loading takes decides which update gameplay resumes on.
/// </summary>
internal static class GameLoadingDefinitions
{
    /// <summary>
    /// Starting gameplay lets the music queue settle three times, twenty updates each.
    /// </summary>
    public const int MusicQueueWaits = 3 * 20;

    /// <summary>
    /// Updates a load spends setting up the room's enemies before gameplay starts (enemy setup plus one update's wait
    /// while its counter runs from six to zero), used by every load except the Zebes landing. The updates themselves
    /// are kept because how long loading takes decides which update gameplay resumes on; the enemies' pictures need
    /// no time to appear.
    /// </summary>
    public const int EnemySetupWaits = 7;

    /// <summary>Updates the Zebes landing spends setting up enemies: its counter runs from fifteen to zero.</summary>
    public const int ZebesLandingEnemySetupWaits = 16;

    /// <summary>Total waiting updates following an ordinary or Ceres load.</summary>
    public const int OrdinaryLoadWaits = MusicQueueWaits + EnemySetupWaits;

    /// <summary>Total waiting updates following the post-Ceres Zebes landing's load.</summary>
    public const int ZebesLandingLoadWaits = MusicQueueWaits + ZebesLandingEnemySetupWaits;
}
