using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // Before a room exists, the frontend still owns the cartridge's live RNG word.
    // Lazy initialization also supports older debugger snapshots without this field.
    private Bank80SystemState? menuRandom;

    private Bank80SystemState FrontendRandomOwner =>
        runtime?.System ?? (menuRandom ??= new Bank80SystemState());

    internal ushort DispatcherRandomNumber => FrontendRandomOwner.RandomNumber;

    private void ReleaseRuntimePreservingRandom()
    {
        (menuRandom ??= new()).SetRandomNumber(FrontendRandomOwner.RandomNumber);
        runtime = null;
    }

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
                // State zero is itself dispatched by MainGameLoop after its first
                // GenerateRandomNumber call; it is not the reset vector's seed store.
                menuRandom.NextRandom();
                runtime?.System.SetRandomNumber(menuRandom.RandomNumber);
                break;
            case SuperMetroidGameState.OpeningCinematic:
            case SuperMetroidGameState.GameOptionsMenu:
            case SuperMetroidGameState.FileSelectMenus:
            case SuperMetroidGameState.FileSelectMap:
            case SuperMetroidGameState.IntroCinematic:
            case SuperMetroidGameState.SetUpNewGame:
            case SuperMetroidGameState.GameOverMenu:
                FrontendRandomOwner.NextRandom();
                break;
        }
        // Gameplay retains its existing call after accepted NMI/HDMA processing.
        // Do not advance here for runtime frames or nested NMI-waiting coroutines.
    }
}
