using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The input half of an accepted NMI ($80:9583): the controller latches and the frame counters.</summary>
/// <param name="controller1">The remapped controller-1 latch the action checks read.</param>
/// <param name="physicalController1">The physical controller-1 latch, for code that reads raw <c>$8B/$8F</c> bits.</param>
/// <param name="options">The controller bindings the remapped latch applies.</param>
/// <param name="clock">The accepted, lagged and all-NMI frame counters.</param>
internal sealed class AcceptedFrameInput(
    ControllerInputState controller1,
    ControllerInputState physicalController1,
    GameplayOptions options,
    FrameClock clock)
{
    /// <summary>Latches <paramref name="controller1Input"/> and advances the frame counters.</summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    internal void Latch(ControllerButton controller1Input)
    {
        // Bank-$90/$91 action checks use the seven configurable WRAM masks. Canonicalizing
        // here preserves one shared rising-edge latch and prevents different movement
        // subsystems from interpreting a remap on different frames. Code that tests fixed
        // bits of $8B/$8F instead (the pause menu's L/R and A) reads the physical latch.
        physicalController1.Latch(controller1Input);
        controller1.Latch(options.ControllerBindings.Normalize(controller1Input));

        clock.AdvanceAcceptedFrame();
    }
}
