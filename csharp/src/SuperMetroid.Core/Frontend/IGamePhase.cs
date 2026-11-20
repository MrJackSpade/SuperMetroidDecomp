using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One sequence's update and the facts the game reads around it. A sequence of several steps answers each fact for the
/// step it is on.
/// </summary>
internal interface IGamePhase
{
    /// <summary>The sequence this phase runs.</summary>
    SuperMetroidGameState State { get; }

    /// <summary>The random generator's step before the sequence's current step runs.</summary>
    PhaseRandomStep RandomStep { get; }

    /// <summary>The audio facts for the sequence's current step, before and after each of its updates.</summary>
    GamePhaseAudio Audio { get; }

    /// <summary>The sequence's current status for hosts and diagnostics.</summary>
    string Name { get; }

    /// <summary>
    /// True when the sequence's current step is waiting out a frame of the display, so the next update finishes it instead
    /// of starting a new update (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    bool IsWaitingOutDisplayFrame { get; }

    /// <summary>What the sequence's current step runs on: nothing of gameplay, the session's runtime, or its loaded room.</summary>
    PhaseGameplayNeed Need { get; }
}

/// <summary>A sequence whose steps run without gameplay.</summary>
internal interface IFrontendPhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.Nothing;

    /// <summary>Runs one update of the sequence.</summary>
    void Step(ControllerButton controllerInput);
}

/// <summary>A sequence whose steps run on the session's gameplay runtime, before or after its room.</summary>
internal interface IRuntimePhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.Runtime;

    /// <summary>Runs one update of the sequence on <paramref name="runtime"/>.</summary>
    void Step(ControllerButton controllerInput, SuperMetroidRuntime runtime);
}

/// <summary>A sequence whose steps run gameplay in the runtime's loaded room.</summary>
internal interface IGameplayPhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.LoadedRoom;

    /// <summary>Runs one update of the sequence in <paramref name="live"/>.</summary>
    void Step(ControllerButton controllerInput, LoadedGameplay live);
}
