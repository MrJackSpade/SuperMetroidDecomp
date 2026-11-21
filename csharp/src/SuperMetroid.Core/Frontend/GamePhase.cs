using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The phase of a sequence made of a single step, whose update and status its owner supplies. It has no special audio
/// handling and never waits out a frame of the display.
/// </summary>
/// <param name="state">The sequence.</param>
/// <param name="randomStep">The random generator's step before the sequence runs.</param>
/// <param name="step">One update of the sequence.</param>
/// <param name="name">The current status.</param>
internal sealed partial class GamePhase(
    SuperMetroidGameState state,
    PhaseRandomStep randomStep,
    Action<ControllerButton> step,
    Func<string> name) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State { get; } = state;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep { get; } = randomStep;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => name();

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput) => step(controllerInput);
}
