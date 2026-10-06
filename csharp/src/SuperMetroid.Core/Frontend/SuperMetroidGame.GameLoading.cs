namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // NMI continuations still owed by the current $82:8000 loading dispatch, and the
    // dispatcher state it selects after its final wait returns.
    private int gameLoadingWaitsRemaining;
    private GameLoadingCompletion gameLoadingCompletion;

    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather
    /// than a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesGameLoadingWait => gameLoadingWaitsRemaining > 0;

    /// <summary>
    /// Records the NMI waits the native loading dispatch performs after the host has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waits);
        gameLoadingWaitsRemaining = waits;
        gameLoadingCompletion = completion;
        PublishBlack();
    }

    /// <summary>
    /// One native loading wait: the accepted NMI reads the controller and performs its
    /// transfers; no dispatcher runs. The last wait returns into the state store.
    /// </summary>
    private void StepGameLoadingWait(ushort controllerInput)
    {
        runtime!.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        if (--gameLoadingWaitsRemaining != 0)
        {
            PublishBlack();
            return;
        }
        switch (gameLoadingCompletion)
        {
            case GameLoadingCompletion.CeresArrivalFadeIn:
                BeginGameplayFadeIn(leadsToCeresArrival: true);
                PublishBlack();
                break;
            case GameLoadingCompletion.GameplayFadeIn:
                BeginGameplayFadeIn(leadsToCeresArrival: false);
                PublishBlack();
                break;
            case GameLoadingCompletion.MainGameplay:
                GameState = SuperMetroidGameState.MainGameplay;
                PublishGameplay(runtime);
                break;
            default:
                throw new InvalidDataException($"Invalid game-loading completion {gameLoadingCompletion}.");
        }
    }
}

/// <summary>Dispatcher state selected when a native loading dispatch finishes its waits.</summary>
/// <remarks>
/// <see cref="GameplayFadeIn"/> is zero: legacy debugger states lacking this field could only
/// be inside the post-Ceres load, which always ends in the ordinary gameplay fade-in.
/// </remarks>
internal enum GameLoadingCompletion
{
    GameplayFadeIn,
    CeresArrivalFadeIn,
    MainGameplay,
}
