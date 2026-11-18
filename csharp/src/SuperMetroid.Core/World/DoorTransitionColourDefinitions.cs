namespace SuperMetroid.Core.World;

/// <summary>The timing of a door's fades (#1405, #1512), as the next room's loading runs them.</summary>
public static class DoorTransitionColourDefinitions
{
    /// <summary>
    /// The shared fade counter's last value at which a step of a door's fades moves the display: the step after it completes
    /// the fade. The source room fades out, and the room it enters fades in, in this many parts, one for each of the fade's
    /// own steps after its first (#2421).
    /// </summary>
    public const int FadeLastStep = 13;
}
