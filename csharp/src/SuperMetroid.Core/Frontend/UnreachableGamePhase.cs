using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A native state no translated producer writes. GameState has a private setter, so reaching one means corrupt internal
/// state; its dispatch fails loudly.
/// </summary>
/// <param name="state">The unused state.</param>
internal sealed class UnreachableGamePhase(SuperMetroidGameState state) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State { get; } = state;

    /// <inheritdoc/>
    public MainLoopRandomCall RandomCall => MainLoopRandomCall.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => State.ToString();

    /// <inheritdoc/>
    public bool ResumesNmiWait => false;

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">Always: the state cannot be dispatched.</exception>
    public void Step(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio) =>
        throw new InvalidDataException(
            $"Frontend dispatcher received invalid state " +
            $"${((ushort)State):X2} ({State}).");
}
