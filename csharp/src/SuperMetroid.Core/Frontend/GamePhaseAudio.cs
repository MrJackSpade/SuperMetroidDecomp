namespace SuperMetroid.Core.Frontend;

/// <summary>The audio facts the main loop's prologue and epilogue read from the dispatched state.</summary>
/// <param name="DoorAudioDispatch">The state belongs to the door transition, whose dispatch can stall on music uploads.</param>
/// <param name="DoorSoundsDisabled">The door has disabled sound effects for this dispatch.</param>
/// <param name="PauseInputEpilogue">$82:8AB0 publishes held input after the pause dispatcher returns.</param>
internal readonly record struct GamePhaseAudio(bool DoorAudioDispatch, bool DoorSoundsDisabled, bool PauseInputEpilogue);
