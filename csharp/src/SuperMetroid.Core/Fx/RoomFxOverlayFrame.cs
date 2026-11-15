namespace SuperMetroid.Core.Fx;

/// <summary>What one effect-overlay FX step reads from the frame: camera, freeze, the power-on rumble delay, RNG and darkness (#633).</summary>
public sealed record RoomFxOverlayFrame
{
    /// <summary>The camera's place in the room this frame.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Whether time is frozen this frame, which holds the effect still.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay.</summary>
    public required PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; init; }

    /// <summary>
    /// The shared random number this frame, 0 through <see cref="SharedRandomState.LargestValue"/>, sampled without
    /// advancing it: its low two bits lengthen the next rising-liquid earthquake-sound delay. A value the shared random
    /// sequence never holds is rejected (#1627).
    /// </summary>
    public int RandomNumber { get; init => field = Ensure.BetweenInclusive(0, SharedRandomState.LargestValue, value); }

    /// <summary>
    /// The room's Fireflea darkness step this frame (none, then one more per Fireflea killed, at most six), copied from
    /// the enemies' room status for the darkness effect; a step the darkness has no level for is rejected.
    /// </summary>
    public int FirefleaDarknessStep { get; init => field = Ensure.BetweenInclusive(0, FirefleaFxDefinitions.DarknessLevels.Length - 1, value); }

    /// <summary>Whether the liquid has already moved this frame, so the effect pass does not move it again.</summary>
    public bool LiquidMotionAlreadyAdvanced { get; init; } = false;
}
