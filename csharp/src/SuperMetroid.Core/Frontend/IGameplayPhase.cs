using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>A sequence whose steps run gameplay in the runtime's loaded room.</summary>
internal interface IGameplayPhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.LoadedRoom;

    /// <summary>Runs one update of the sequence in <paramref name="live"/>.</summary>
    void Step(ControllerButton controllerInput, LoadedGameplay live);
}
