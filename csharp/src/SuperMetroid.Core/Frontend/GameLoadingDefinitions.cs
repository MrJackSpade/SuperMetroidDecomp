namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Native NMI waits inside the single game-loading dispatch of states $06/$1F
/// (<c>$82:8000</c>). Each wait is one accepted NMI with no main-loop dispatch.
/// </summary>
internal static class GameLoadingDefinitions
{
    /// <summary>
    /// <c>StartGameplay</c> ($80:A07B) calls <c>HandleMusicQueueFor20Frames</c> ($80:A12B)
    /// from $80:A0A7, $80:A0D2 and $80:A117; each call waits for $14 NMIs.
    /// </summary>
    public const int MusicQueueWaits = 3 * 0x14;

    /// <summary>
    /// Frames a load spends setting up the room's enemies before gameplay starts ($82:80F6: enemy initialization plus
    /// one frame's wait while its counter runs from six to zero), used by every load except the Zebes landing. The
    /// frames themselves are kept because how long loading takes decides which frame gameplay resumes on; the
    /// enemies' pictures need no time to appear.
    /// </summary>
    public const int EnemySetupWaits = 7;

    /// <summary>Frames the Zebes landing ($82:80C3, state $22) spends setting up enemies: its counter runs from fifteen to zero.</summary>
    public const int ZebesLandingEnemySetupWaits = 16;

    /// <summary>Total NMI continuations following an ordinary or Ceres loading dispatch.</summary>
    public const int OrdinaryLoadWaits = MusicQueueWaits + EnemySetupWaits;

    /// <summary>Total NMI continuations following the post-Ceres Zebes-landing dispatch.</summary>
    public const int ZebesLandingLoadWaits = MusicQueueWaits + ZebesLandingEnemySetupWaits;
}
