using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>One game state's update and the per-state facts the game reads around it.</summary>
internal interface IGamePhase
{
    /// <summary>The state this phase dispatches.</summary>
    SuperMetroidGameState State { get; }

    /// <summary>The random generator's step before this state's dispatch.</summary>
    DispatchRandomStep RandomStep { get; }

    /// <summary>The prologue and epilogue audio facts for this state.</summary>
    GamePhaseAudio Audio { get; }

    /// <summary>The state's current status for hosts and diagnostics.</summary>
    string Name { get; }

    /// <summary>
    /// True when this state's current step is waiting out a frame of the display, so the next update finishes it instead of
    /// starting a new update (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    bool IsWaitingOutDisplayFrame { get; }

    /// <summary>Runs one dispatch of the state.</summary>
    void Step(ControllerButton controllerInput);
}
