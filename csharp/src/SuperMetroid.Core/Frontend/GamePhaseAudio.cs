namespace SuperMetroid.Core.Frontend;

/// <summary>The audio handling the main loop's prologue and epilogue give the dispatched state.</summary>
internal enum GamePhaseAudio
{
    /// <summary>No special handling.</summary>
    Ordinary,

    /// <summary>A door-transition state, whose dispatch can stall on music uploads.</summary>
    DoorTransition,

    /// <summary>A door-transition state that has also disabled sound effects for this dispatch.</summary>
    DoorTransitionSoundsDisabled,

    /// <summary>A pause state: $82:8AB0 publishes held input after the pause dispatcher returns.</summary>
    PauseInputEpilogue,
}
