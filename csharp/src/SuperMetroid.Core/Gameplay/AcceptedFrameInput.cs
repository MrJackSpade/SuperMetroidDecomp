using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The input half of a frame boundary: the player's action and button latches and the frame counters.</summary>
/// <param name="playerActions">The gameplay actions latch the action checks read.</param>
/// <param name="playerButtons">The player's physical button latch, for code that reads fixed buttons.</param>
/// <param name="options">The controller bindings that read the buttons as actions.</param>
/// <param name="clock">The frame counters.</param>
internal sealed class AcceptedFrameInput(
    GameplayActionInput playerActions,
    ControllerInputState playerButtons,
    GameplayOptions options,
    FrameClock clock)
{
    /// <summary>Latches <paramref name="playerInput"/> and advances the frame counters.</summary>
    /// <param name="playerInput">The host's controller buttons.</param>
    internal void Latch(ControllerButton playerInput)
    {
        // Gameplay reads actions through the seven configurable bindings. Reading them
        // here preserves one shared rising-edge latch and prevents different movement
        // subsystems from interpreting a remap on different frames. Code that tests fixed
        // buttons instead (the pause menu's L/R and A) reads the physical latch.
        playerButtons.Latch(playerInput);
        playerActions.Latch(options.ControllerBindings.ToActions(playerInput));

        clock.AdvanceAcceptedFrame();
    }
}
