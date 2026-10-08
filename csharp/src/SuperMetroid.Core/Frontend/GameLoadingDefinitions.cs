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
    /// $82:80F6 loop: <c>TransferEnemyTilesToVRAM_InitialiseEnemies</c> plus one NMI wait
    /// while the counter runs from six to zero, used by every load except the Zebes landing.
    /// </summary>
    public const int EnemyTileTransferWaits = 7;

    /// <summary>$82:80C3 loop: the Zebes-landing ($22) branch runs the counter from $0F to zero.</summary>
    public const int ZebesLandingEnemyTileTransferWaits = 16;

    /// <summary>Total NMI continuations following an ordinary or Ceres loading dispatch.</summary>
    public const int OrdinaryLoadWaits = MusicQueueWaits + EnemyTileTransferWaits;

    /// <summary>Total NMI continuations following the post-Ceres Zebes-landing dispatch.</summary>
    public const int ZebesLandingLoadWaits = MusicQueueWaits + ZebesLandingEnemyTileTransferWaits;
}
