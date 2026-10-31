using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>What one layer-3 FX step reads from the frame: camera, freeze, the power-on rumble delay, RNG, darkness and the power bomb (#633).</summary>
public sealed record RoomLayer3FxFrame
{
    /// <summary>Camera.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Time is frozen.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay.</summary>
    public required PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; init; }

    /// <summary>
    /// The shared random number this frame, 0 through 65535, sampled without advancing it: its low two bits lengthen
    /// the next rising-liquid earthquake-sound delay. Values outside the random word's range are rejected.
    /// </summary>
    public int RandomNumber { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>
    /// The room's Fireflea darkness level this frame (0, then +2 per Fireflea killed, at most 12), copied from the
    /// enemies' room status for the darkness effect, which already takes it as a number; values outside the
    /// cartridge's word (0 through 65535) are rejected.
    /// </summary>
    public int FirefleaDarknessLevel { get; init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Power bomb.</summary>
    public IActivePowerBomb? PowerBomb { get; init; } = null;

    /// <summary>Liquid motion already advanced.</summary>
    public bool LiquidMotionAlreadyAdvanced { get; init; } = false;
}
