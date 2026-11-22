using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The game load: the chosen game, a continued save's checkpoint or the Zebes landing loads in one update, then the black
/// screen holds for the load's waits.
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed partial class LoadingGameSequence(FrontendSession session) : IFrontendPhase, IRuntimePhase, IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.LoadingGame;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <summary>The diagnostic name of each load route, read from the session it runs against.</summary>
    private static IdentityTable<GameDataLoadRoute, Func<FrontendSession, string>> Names => field ??= new(new Dictionary<GameDataLoadRoute, Func<FrontendSession, string>>
    {
        [GameDataLoadRoute.SelectedGame] = static running => running.LoadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
        [GameDataLoadRoute.SavedCheckpoint] = static running => $"Loading saved checkpoint ({running.GameLoading.Wait.UpdatesLeft} updates)",
        [GameDataLoadRoute.ZebesLanding] = static running => $"Loading Landing Site ({running.GameLoading.Wait.UpdatesLeft} updates)",
    });

    /// <inheritdoc/>
    public string Name => Names[session.LoadingRoute](session);

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => session.ResumesGameLoadingWait;

    /// <summary>What <c>Step</c> does for each <see cref="GameDataLoadRoute"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<GameDataLoadRoute, Action<ControllerButton>> StepForGameDataLoadRoute => field ??= new(new Dictionary<GameDataLoadRoute, Action<ControllerButton>>
    {
        [GameDataLoadRoute.SelectedGame] = controllerInput =>
        {
            // The whole load happens in one update, then the black screen holds for its waits.
            bool usesCeresArrival = session.SetupSelectedGame();
            BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits, usesCeresArrival
                ? GameLoadingCompletion.CeresArrivalFadeIn
                : GameLoadingCompletion.MainGameplay);
        },
        [GameDataLoadRoute.SavedCheckpoint] = controllerInput =>
        {
            // A save continued from the file-select map: the ordinary load brings in the
            // game in one update, then holds for the same waits as a new game.
            if (session.SetupSelectedGame())
                throw new InvalidOperationException("A save continued from the area map cannot use the Ceres arrival.");
            BeginGameLoadingWaits(GameLoadingDefinitions.OrdinaryLoadWaits,
                GameLoadingCompletion.GameplayFadeIn);
        },
        // The Zebes landing runs in the runtime Ceres left (Step with the runtime), never without one.
        [GameDataLoadRoute.ZebesLanding] = controllerInput => throw new InvalidOperationException(
            $"The {GameDataLoadRoute.ZebesLanding} load runs in the runtime Ceres left."),
    });

    /// <inheritdoc/>
    public PhaseGameplayNeed Need => session.ResumesGameLoadingWait ? PhaseGameplayNeed.LoadedRoom
        : session.LoadingRoute == GameDataLoadRoute.ZebesLanding ? PhaseGameplayNeed.Runtime
        : PhaseGameplayNeed.Nothing;

    /// <inheritdoc/>
    /// <remarks>The load's waits hold the room it loaded.</remarks>
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => StepGameLoadingWait(controllerInput, live);

    /// <inheritdoc/>
    /// <remarks>
    /// The Zebes landing, followed by its waits. The update's own frame counts in the gameplay owner kept from Ceres, in the
    /// runtime Ceres left.
    /// </remarks>
    public void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        runtime.RoomLoader.InitializePostCeresZebesRoom();
        BeginGameLoadingWaits(GameLoadingDefinitions.ZebesLandingLoadWaits,
            GameLoadingCompletion.GameplayFadeIn);
    }

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        StepForGameDataLoadRoute[session.LoadingRoute](controllerInput);
    }

    /// <summary>
    /// Records the waits the game holds after it has
    /// completed the loading work itself.
    /// </summary>
    private void BeginGameLoadingWaits(int waits, GameLoadingCompletion completion)
    {
        session.GameLoading.Begin(waits, completion);
        session.display.PublishBlack();
    }

    /// <summary>What <see cref="StepGameLoadingWait"/> does for each <see cref="GameLoadingCompletion"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<GameLoadingCompletion, Action<LoadedGameplay>> StepGameLoadingWaitForGameLoadingCompletion => field ??= new(new Dictionary<GameLoadingCompletion, Action<LoadedGameplay>>
    {
        [GameLoadingCompletion.CeresArrivalFadeIn] = live =>
        {
            session.BeginGameplayFadeIn(target: GameplayFadeTarget.CeresArrival);
            session.display.PublishBlack();
        },
        [GameLoadingCompletion.GameplayFadeIn] = live =>
        {
            session.BeginGameplayFadeIn(target: GameplayFadeTarget.Gameplay);
            session.display.PublishBlack();
        },
        [GameLoadingCompletion.MainGameplay] = live =>
        {
            session.Enter(session.Phases.MainGameplay);
            session.display.PublishGameplay(live);
        },
    });

    /// <summary>
    /// One loading wait: the controller is read and the display
    /// updated; nothing else runs. The last wait enters what the load completes into.
    /// </summary>
    private void StepGameLoadingWait(ControllerButton controllerInput, LoadedGameplay live)
    {
        live.Runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (!session.GameLoading.CountWait())
        {
            session.display.PublishBlack();
            return;
        }
        StepGameLoadingWaitForGameLoadingCompletion[session.GameLoading.Completion](live);
    }
}
