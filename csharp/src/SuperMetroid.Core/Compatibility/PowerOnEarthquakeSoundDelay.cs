namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The frame the save file found at power-on adds to every earthquake rumble's delay (#1475). At power-on the game
/// checks the save file's completion mark, and the outcome of that check stays with the whole session's play: the
/// rumble that a rising liquid and the Tourian statues' descent repeat waits a random zero to three frames past its base
/// delay, plus one more frame when power-on found no valid save, a completed game, or the mark boot writes over a
/// corrupt save file. A session that found a valid save without either mark waits no extra frame.
/// </summary>
/// <remarks>
/// Consulted each time a rising liquid's or the Tourian statues' earthquake rumble is queued and its next delay is
/// reloaded, during the room's layer-3 effect step; fixed at power-on for the session and injected into every gameplay
/// runtime it creates. Evidence: <c>DetermineNumberOfDemoSets</c> ($80:8261) returns the carry of comparing the marker
/// field with the completion signature word by word from the last (set when no slot is valid), <c>MainGameLoop</c>
/// ($82:8948) brackets every pass with <c>PHP</c>/<c>PLP</c> so it keeps that carry, and the rumble's delay reload at
/// $88:B245 adds with <c>ADC</c> without clearing it; the main-game-loop-carry check reproduces each power-on case. A
/// legacy save image whose marker holds other bytes leaves the outcome its raw comparison gives until a save rewrites the
/// mark; the host's save codec decides it.
/// </remarks>
[CompatibilityService("the save file found at power-on adds one frame to every rising-liquid and Tourian-statue earthquake rumble's delay")]
public sealed class PowerOnEarthquakeSoundDelay
{
    /// <summary>A session whose power-on check adds no frame.</summary>
    public static PowerOnEarthquakeSoundDelay None { get; } = new(extraFrames: 0);

    /// <summary>A session whose power-on check adds one frame.</summary>
    public static PowerOnEarthquakeSoundDelay OneFrame { get; } = new(extraFrames: 1);

    /// <summary>Creates the delay of one power-on outcome.</summary>
    private PowerOnEarthquakeSoundDelay(int extraFrames) => ExtraFrames = extraFrames;

    /// <summary>The frames, zero or one, every earthquake rumble's delay gains.</summary>
    public int ExtraFrames { get; }
}
