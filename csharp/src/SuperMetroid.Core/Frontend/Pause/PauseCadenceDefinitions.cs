using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The counter phases of the pause screen's reserve tank display (#1606): the AUTO arrow's pulse and the flickering fill step.</summary>
internal static class PauseCadenceDefinitions
{
    /// <summary>
    /// The pause map and item screens' held-button delay: the unchanged updates a held button waits through before it moves
    /// the cursor again, on the next update and every update after.
    /// </summary>
    internal const int HeldInputRepeatDelay = 3;

    /// <summary>
    /// The phase of the pause screen's frame counter that picks the pulsing AUTO arrow's picture: one of
    /// its <see cref="PauseReserveArrowDefinitions.ArrowFrames"/> pulse frames, one an update in turn.
    /// </summary>
    internal static readonly UpdateCounterField ReserveArrowPulseFrame = new(0, 5);

    /// <summary>
    /// The phase of the pause screen's frame counter on which a partly filled reserve tank hides its
    /// flickering next step: set for four updates of every eight; the step shows for the other four.
    /// </summary>
    internal static readonly UpdateCounterField ReserveFlickerStepHidden = new(2, 1);
}
