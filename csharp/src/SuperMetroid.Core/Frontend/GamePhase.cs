using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>A game state's phase whose update and status are supplied by its owner.</summary>
/// <param name="state">The state.</param>
/// <param name="randomStep">The random generator's step before the phase runs.</param>
/// <param name="step">One update of the state.</param>
/// <param name="name">The current status.</param>
/// <param name="waitingOutDisplayFrame">Whether the state's current step is waiting out a frame of the display, so the next update finishes it; never when omitted.</param>
/// <param name="audio">The prologue and epilogue audio handling.</param>
internal sealed class GamePhase(
    SuperMetroidGameState state,
    PhaseRandomStep randomStep,
    Action<ControllerButton> step,
    Func<string> name,
    Func<bool>? waitingOutDisplayFrame = null,
    GamePhaseAudio audio = GamePhaseAudio.Ordinary) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State { get; } = state;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep { get; } = randomStep;

    /// <inheritdoc/>
    public GamePhaseAudio Audio { get; } = audio;

    /// <inheritdoc/>
    public string Name => name();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => waitingOutDisplayFrame?.Invoke() == true;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput) => step(controllerInput);
}
