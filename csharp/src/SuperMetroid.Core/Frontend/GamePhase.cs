using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>A game state's phase whose dispatch and status are supplied by its owner.</summary>
/// <param name="state">The state.</param>
/// <param name="randomStep">The random generator's step before the dispatch.</param>
/// <param name="step">One dispatch.</param>
/// <param name="name">The current status.</param>
/// <param name="resumesFrameWait">Whether the next update continues a step that waits out a frame of the display; never when omitted.</param>
/// <param name="audio">The prologue and epilogue audio handling.</param>
internal sealed class GamePhase(
    SuperMetroidGameState state,
    DispatchRandomStep randomStep,
    Action<ControllerButton, GameplayAudioFramePublication> step,
    Func<string> name,
    Func<bool>? resumesFrameWait = null,
    GamePhaseAudio audio = GamePhaseAudio.Ordinary) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State { get; } = state;

    /// <inheritdoc/>
    public DispatchRandomStep RandomStep { get; } = randomStep;

    /// <inheritdoc/>
    public GamePhaseAudio Audio { get; } = audio;

    /// <inheritdoc/>
    public string Name => name();

    /// <inheritdoc/>
    public bool ResumesFrameWait => resumesFrameWait?.Invoke() == true;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio) => step(controllerInput, gameplayAudio);
}
