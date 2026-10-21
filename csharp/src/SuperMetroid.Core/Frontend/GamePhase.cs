using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>A game state's phase whose dispatch and status are supplied by its owner.</summary>
/// <param name="state">The state.</param>
/// <param name="randomCall">The main loop's RNG call before the dispatch.</param>
/// <param name="step">One dispatch.</param>
/// <param name="name">The current status.</param>
/// <param name="resumesNmiWait">Whether the next update resumes inside the dispatch's NMI wait; never when omitted.</param>
/// <param name="audio">The prologue and epilogue audio handling.</param>
internal sealed class GamePhase(
    SuperMetroidGameState state,
    MainLoopRandomCall randomCall,
    Action<SnesButton, GameplayAudioFramePublication> step,
    Func<string> name,
    Func<bool>? resumesNmiWait = null,
    GamePhaseAudio audio = GamePhaseAudio.Ordinary) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State { get; } = state;

    /// <inheritdoc/>
    public MainLoopRandomCall RandomCall { get; } = randomCall;

    /// <inheritdoc/>
    public GamePhaseAudio Audio { get; } = audio;

    /// <inheritdoc/>
    public string Name => name();

    /// <inheritdoc/>
    public bool ResumesNmiWait => resumesNmiWait?.Invoke() == true;

    /// <inheritdoc/>
    public void Step(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio) => step(controllerInput, gameplayAudio);
}
