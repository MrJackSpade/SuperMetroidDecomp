using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>A sequence whose steps run without gameplay.</summary>
internal interface IFrontendPhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.Nothing;

    /// <summary>Runs one update of the sequence.</summary>
    void Step(ControllerButton controllerInput);
}
