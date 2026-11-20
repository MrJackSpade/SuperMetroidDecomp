namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The frame the save file found at power-on adds to every earthquake rumble's delay (#1475, #2140). At power-on the game
/// checks the save file, and the outcome of that check stays with the whole session's play: the rumble that a rising
/// liquid and the Tourian statues' descent repeat waits a random zero to three frames past its base delay, plus one more
/// frame when power-on found no valid save, or a save file whose history holds a completed game or an earlier power-on
/// that found no valid save. A session that found a valid save without either waits no extra frame.
/// </summary>
/// <remarks>
/// Consulted each time a rising liquid's or the Tourian statues' earthquake rumble is queued and its next delay is
/// reloaded, during the room's overlay effect step; fixed at power-on for the session and injected into every gameplay
/// runtime it creates. Evidence: <c>DetermineNumberOfDemoSets</c> ($80:8261) returns the carry of comparing the save
/// file's marker with the completed-game signature word by word from the last (set when no slot is valid),
/// <c>MainGameLoop</c> ($82:8948) brackets every pass with <c>PHP</c>/<c>PLP</c> so it keeps that carry, and the
/// rumble's delay reload at $88:B245 adds with <c>ADC</c> without clearing it; the main-game-loop-carry check reproduces
/// each power-on case. The comparison leaves the carry set for a completed game and for a power-on that found no valid
/// save, and clear when the save file's history holds neither.
/// </remarks>
[CompatibilityService("the save file found at power-on adds one frame to every rising-liquid and Tourian-statue earthquake rumble's delay")]
public sealed class PowerOnEarthquakeSoundDelay
{
    /// <summary>A session whose power-on check adds no frame.</summary>
    public static PowerOnEarthquakeSoundDelay None { get; } = new(extraFrames: 0);

    /// <summary>A session whose power-on check adds one frame.</summary>
    private static PowerOnEarthquakeSoundDelay OneFrame { get; } = new(extraFrames: 1);

    /// <summary>The delay a session gets from what its power-on found in the save file.</summary>
    /// <param name="validSaveFound">Whether any slot held a valid save.</param>
    /// <param name="gameCompleted">Whether the save file's history holds a completed game.</param>
    /// <param name="powerOnFoundNoValidSave">Whether the save file's history holds an earlier power-on that found no valid save.</param>
    /// <returns><see cref="OneFrame"/> when no slot is valid or the history holds either; otherwise <see cref="None"/>.</returns>
    [IndependentFlag("Power-on reads three facts from the save file: whether a slot is valid, and which of the two its history holds, if either.")]
    public static PowerOnEarthquakeSoundDelay FoundAtPowerOn(bool validSaveFound, bool gameCompleted, bool powerOnFoundNoValidSave) =>
        !validSaveFound || gameCompleted || powerOnFoundNoValidSave ? OneFrame : None;

    /// <summary>Creates the delay of one power-on outcome.</summary>
    private PowerOnEarthquakeSoundDelay(int extraFrames) => ExtraFrames = extraFrames;

    /// <summary>The frames, zero or one, every earthquake rumble's delay gains.</summary>
    public int ExtraFrames { get; }
}
