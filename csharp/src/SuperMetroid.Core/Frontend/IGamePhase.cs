using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>One native game state's dispatch and the per-state facts the main loop reads around it.</summary>
internal interface IGamePhase
{
    /// <summary>The state this phase dispatches.</summary>
    SuperMetroidGameState State { get; }

    /// <summary>The main loop's RNG call before this state's dispatch.</summary>
    MainLoopRandomCall RandomCall { get; }

    /// <summary>The prologue and epilogue audio facts for this state.</summary>
    GamePhaseAudio Audio { get; }

    /// <summary>The state's current status for hosts and diagnostics.</summary>
    string Name { get; }

    /// <summary>True when the next update resumes this state's dispatch after its own NMI wait instead of entering the main loop.</summary>
    bool ResumesNmiWait { get; }

    /// <summary>Runs one dispatch of the state.</summary>
    void Step(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio);
}
