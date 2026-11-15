namespace SuperMetroid.Core.Fx;

/// <summary>What one effect-overlay FX step reads from the frame: camera, freeze and darkness (#633).</summary>
public sealed record RoomFxOverlayFrame
{
    /// <summary>Camera.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Time is frozen.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>
    /// The room's Fireflea darkness step this frame (none, then one more per Fireflea killed, at most six), copied from
    /// the enemies' room status for the darkness effect; a step the darkness has no level for is rejected.
    /// </summary>
    public int FirefleaDarknessStep { get; init => field = Ensure.BetweenInclusive(0, FirefleaFxDefinitions.DarknessLevels.Length - 1, value); }
}
