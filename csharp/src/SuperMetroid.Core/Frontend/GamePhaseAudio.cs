namespace SuperMetroid.Core.Frontend;

/// <summary>The audio handling the update gives the running state before and after its phase.</summary>
internal enum GamePhaseAudio
{
    /// <summary>No special handling.</summary>
    Ordinary,

    /// <summary>A door-transition state, whose update can wait while a new music set loads.</summary>
    DoorTransition,

    /// <summary>A door-transition state that has also disabled sound effects for this dispatch.</summary>
    DoorTransitionSoundsDisabled,

    /// <summary>A pause state: held input is published after the pause phase returns.</summary>
    PauseInputEpilogue,
}
