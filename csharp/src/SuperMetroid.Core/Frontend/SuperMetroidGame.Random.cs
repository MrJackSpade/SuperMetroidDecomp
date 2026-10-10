using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // Before a room exists, the frontend still owns the cartridge's live RNG word.
    // Lazy initialization also supports older debugger snapshots without this field.
    private Bank80SystemState? menuRandom;

    private Bank80SystemState FrontendRandomOwner =>
        runtime?.System ?? (menuRandom ??= new Bank80SystemState());

    private void ReleaseRuntimePreservingRandom()
    {
        (menuRandom ??= new()).SetRandomNumber(FrontendRandomOwner.RandomNumber);
        // File-map selection may release when no runtime was ever created.
        if (runtime is not null)
            RetainRuntimeNmiFrameCounters();
        runtime = null;
    }

    /// <summary>
    /// True when the next update resumes a frontend dispatch after its own NMI wait instead
    /// of entering MainGameLoop. Such an update makes no main-loop RNG call.
    /// </summary>
    internal bool NextUpdateResumesNmiWait => GameState switch
    {
        SuperMetroidGameState.FileSelectMenus => fileSelect?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.SetUpNewGame or SuperMetroidGameState.LoadingGameData => ResumesGameLoadingWait,
        SuperMetroidGameState.CeresGoesBoom or SuperMetroidGameState.CeresGoesBoomWithSamus =>
            ceresDestruction?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.EndingAndCredits => endingCredits?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.GameOverMenu => gameOver?.ResumesAfterNmiWait == true,
        SuperMetroidGameState.Reset or SuperMetroidGameState.OpeningCinematic or
            SuperMetroidGameState.GameOptionsMenu or SuperMetroidGameState.Unused03 or
            SuperMetroidGameState.FileSelectMap or SuperMetroidGameState.MainGameplayFadeIn or
            SuperMetroidGameState.MainGameplay or SuperMetroidGameState.HitDoorBlock or
            SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB or
            SuperMetroidGameState.PausingDarkening or SuperMetroidGameState.Pausing or
            SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
            SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB or
            SuperMetroidGameState.Unpausing or SuperMetroidGameState.DeathSequenceStart or
            SuperMetroidGameState.DeathBlackOutSurroundings or SuperMetroidGameState.DeathWaitForMusic or
            SuperMetroidGameState.DeathPreFlashing or SuperMetroidGameState.DeathFlashing or
            SuperMetroidGameState.DeathExplosionWhiteOut or SuperMetroidGameState.DeathFinalBlackOut or
            SuperMetroidGameState.ReserveTanksAuto or SuperMetroidGameState.Unused1c or
            SuperMetroidGameState.DebugGameOverMenu or SuperMetroidGameState.IntroCinematic or
            SuperMetroidGameState.MadeItToCeresElevator or SuperMetroidGameState.BlackoutFromCeres or
            SuperMetroidGameState.TimeUp or SuperMetroidGameState.WhitingOutFromTimeUp or
            SuperMetroidGameState.SamusEscapesFromZebes or SuperMetroidGameState.TransitionToDemoA or
            SuperMetroidGameState.TransitionToDemoB or SuperMetroidGameState.PlayingDemo or
            SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB => false,
        _ => throw new InvalidOperationException($"Undefined SuperMetroidGameState {GameState}."),
    };

    /// <summary>
    /// Preserves the bank-$82 main-loop RNG call through title/file selection and
    /// loading. These menu dispatchers return once per frame; they do not run the
    /// gameplay runtime's prologue. SRAM contains no copy of the live RNG word.
    /// </summary>
    private void AdvanceMenuRandom()
    {
        switch (GameState)
        {
            case SuperMetroidGameState.Reset:
                // Only the reset vector reseeds. A continued save must keep the word
                // produced by gameplay and the game-over/file-map waiting frames.
                menuRandom = new Bank80SystemState();
                ResetMenuNmiFrameCounters();
                // State zero is itself dispatched by MainGameLoop after its first
                // GenerateRandomNumber call; it is not the reset vector's seed store.
                menuRandom.NextRandom();
                runtime?.System.SetRandomNumber(menuRandom.RandomNumber);
                break;
            case SuperMetroidGameState.Pausing:
            // $24 fades without a gameplay call; its HDMA objects still run until it clears them.
            case SuperMetroidGameState.WhitingOutFromTimeUp:
                // HDMA runs before $0D disables it; lava swaps the live RNG bytes.
                runtime!.AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true);
                break;
            case SuperMetroidGameState.FileSelectMenus:
            case SuperMetroidGameState.SetUpNewGame:
            case SuperMetroidGameState.LoadingGameData:
            // $82:8B0E shares the intro's state-$1E/$22/$25 handler and its prologue call.
            case SuperMetroidGameState.CeresGoesBoom:
            case SuperMetroidGameState.CeresGoesBoomWithSamus:
            // State $27 is dispatched by MainGameLoop after its RNG call, except while the
            // ending setup resumes inside its own NMI waits.
            case SuperMetroidGameState.EndingAndCredits:
            // Game-over indexes zero and one resume inside their own NMI waits.
            case SuperMetroidGameState.GameOverMenu:
                if (!NextUpdateResumesNmiWait)
                    FrontendRandomOwner.NextRandom();
                break;
            case SuperMetroidGameState.OpeningCinematic:
            case SuperMetroidGameState.GameOptionsMenu:
            case SuperMetroidGameState.FileSelectMap:
            case SuperMetroidGameState.IntroCinematic:
            // State $19 only fades; MainGameLoop's RNG call precedes it as any dispatch.
            case SuperMetroidGameState.DeathFinalBlackOut:
            // $82:894F runs once before each pause-only dispatcher as well.
            // Fade states $0C/$12 run StepFrame and retain its own RNG call.
            case SuperMetroidGameState.PausedA:
            case SuperMetroidGameState.PausedB:
            case SuperMetroidGameState.UnpausingA:
            case SuperMetroidGameState.UnpausingB:
                FrontendRandomOwner.NextRandom();
                break;
            // The gameplay runtime, or no RNG call, owns these states.
            case SuperMetroidGameState.Unused03:
            case SuperMetroidGameState.MainGameplayFadeIn:
            case SuperMetroidGameState.MainGameplay:
            case SuperMetroidGameState.HitDoorBlock:
            case SuperMetroidGameState.LoadingNextRoomA:
            case SuperMetroidGameState.LoadingNextRoomB:
            case SuperMetroidGameState.PausingDarkening:
            case SuperMetroidGameState.Unpausing:
            case SuperMetroidGameState.DeathSequenceStart:
            case SuperMetroidGameState.DeathBlackOutSurroundings:
            case SuperMetroidGameState.DeathWaitForMusic:
            case SuperMetroidGameState.DeathPreFlashing:
            case SuperMetroidGameState.DeathFlashing:
            case SuperMetroidGameState.DeathExplosionWhiteOut:
            case SuperMetroidGameState.ReserveTanksAuto:
            case SuperMetroidGameState.Unused1c:
            case SuperMetroidGameState.DebugGameOverMenu:
            case SuperMetroidGameState.MadeItToCeresElevator:
            case SuperMetroidGameState.BlackoutFromCeres:
            case SuperMetroidGameState.TimeUp:
            case SuperMetroidGameState.SamusEscapesFromZebes:
            case SuperMetroidGameState.TransitionToDemoA:
            case SuperMetroidGameState.TransitionToDemoB:
            case SuperMetroidGameState.PlayingDemo:
            case SuperMetroidGameState.TransitionFromDemoA:
            case SuperMetroidGameState.TransitionFromDemoB:
                break;
            default:
                throw new InvalidOperationException($"Undefined SuperMetroidGameState {GameState}.");
        }
        // Gameplay retains its existing call after accepted NMI/HDMA processing.
        // Do not advance here for runtime frames or nested NMI-waiting coroutines.
    }
}
