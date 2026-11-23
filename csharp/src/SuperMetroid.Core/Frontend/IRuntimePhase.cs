using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>A sequence whose steps run on the session's gameplay runtime, before or after its room.</summary>
internal interface IRuntimePhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.Runtime;

    /// <summary>Runs one update of the sequence on <paramref name="runtime"/>.</summary>
    void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime);
}
