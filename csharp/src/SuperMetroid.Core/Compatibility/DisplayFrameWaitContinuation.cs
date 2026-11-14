namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An update that finishes a menu, loading or cinematic step left waiting out a frame of the display is no new update
/// (#1592): it draws no random number before the step, and runs neither the update's music processing nor its sound
/// processing. The file-select menus and map, the game-data loads, the game-over menu, the Ceres destruction cinematics
/// and the ending's setup each have steps that blank or unblank the screen and wait for the next frame in the middle of
/// their work; the update after such a wait only finishes that step. Every other update of those states, and of every
/// state whose random advance the frontend owns, draws its random number first, so the shared random sequence, and
/// every later choice drawn from it, depends on which updates finish a waiting step.
/// </summary>
/// <remarks>
/// Consulted at the start of every update, before the music processing and the frontend's random draw, with whether the
/// current state's step is waiting out a frame of the display. Evidence: the original main loop (<c>$82:8948</c>) runs
/// the music queue, draws its random number (<c>$82:894F</c>) and then calls the game state, which runs the sound
/// queues' passes after it returns; a game-state routine that waits for the next frame inside itself (<c>$80:8338</c>)
/// continues there on that frame without a pass of the loop. The save-load random audit compares the random number on
/// each such update.
/// </remarks>
[CompatibilityService("an update that finishes a menu, loading or cinematic step waiting out a frame of the display draws no random number and runs no music or sound processing")]
public static class DisplayFrameWaitContinuation
{
    /// <summary>Whether the update draws the frontend's random number before its state's step.</summary>
    /// <param name="stepWaitingOutDisplayFrame">Whether the state's current step is waiting out a frame of the display, so this update finishes it.</param>
    public static bool DrawsRandom(bool stepWaitingOutDisplayFrame) => !stepWaitingOutDisplayFrame;

    /// <summary>Whether the update runs its music processing before the state's step and its sound processing after it.</summary>
    /// <param name="stepWaitingOutDisplayFrame">Whether the state's current step is waiting out a frame of the display, so this update finishes it.</param>
    public static bool RunsAudioProcessing(bool stepWaitingOutDisplayFrame) => !stepWaitingOutDisplayFrame;
}
