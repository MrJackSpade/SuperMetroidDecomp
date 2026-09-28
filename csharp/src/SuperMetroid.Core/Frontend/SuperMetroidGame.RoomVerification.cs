namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>
    /// Initialize the ordinary gameplay runtime for a friend verifier that
    /// directly loads a retail room. This uses the same runtime setup as a new
    /// game, then omits only the title/intro/fade dispatcher frames. It is not
    /// an alternate playable entry point or a substitute for the separate
    /// full-startup integration test.
    /// </summary>
    internal void InitializeDirectRoomVerification()
    {
        if (runtime is not null || GameState != SuperMetroidGameState.Reset)
            throw new InvalidOperationException(
                "Direct-room verification requires a fresh frontend.");
        if (!SetupSelectedGame())
            throw new InvalidOperationException(
                "New-game runtime setup did not select the Ceres arrival path.");
        GameState = SuperMetroidGameState.MainGameplay;
    }
}
