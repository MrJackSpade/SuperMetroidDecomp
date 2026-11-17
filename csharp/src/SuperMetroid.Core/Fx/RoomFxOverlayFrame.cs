namespace SuperMetroid.Core.Fx;

/// <summary>What one effect-overlay FX step reads from the frame: camera, freeze and darkness (#633).</summary>
public sealed record RoomFxOverlayFrame
{
    /// <summary>The camera's place in the room this frame.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Whether time is frozen this frame, which holds the effect still.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>
    /// The room's Fireflea darkness this frame, keyed by the Firefleas the enemies' room status counts killed (#2367); none
    /// killed by default.
    /// </summary>
    public FirefleaDarkness FirefleaDarkness { get; init; }
}
