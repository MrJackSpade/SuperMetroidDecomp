using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The input half of a frame boundary: the controller latches and the frame counters.</summary>
/// <param name="controller1">The gameplay actions latch the action checks read.</param>
/// <param name="physicalController1">The physical controller-1 latch, for code that reads the physical buttons.</param>
/// <param name="options">The controller bindings that read the buttons as actions.</param>
/// <param name="clock">The frame counters.</param>
internal sealed class AcceptedFrameInput(
    GameplayActionInput controller1,
    ControllerInputState physicalController1,
    GameplayOptions options,
    FrameClock clock)
{
    /// <summary>Latches <paramref name="controller1Input"/> and advances the frame counters.</summary>
    /// <param name="controller1Input">The host's controller buttons.</param>
    internal void Latch(ControllerButton controller1Input)
    {
        // Gameplay reads actions through the seven configurable bindings. Reading them
        // here preserves one shared rising-edge latch and prevents different movement
        // subsystems from interpreting a remap on different frames. Code that tests fixed
        // buttons instead (the pause menu's L/R and A) reads the physical latch.
        physicalController1.Latch(controller1Input);
        controller1.Latch(options.ControllerBindings.ToActions(controller1Input));

        clock.AdvanceAcceptedFrame();
    }
}
