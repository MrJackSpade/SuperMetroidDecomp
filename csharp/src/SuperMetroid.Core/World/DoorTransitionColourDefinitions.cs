namespace SuperMetroid.Core.World;

/// <summary>The timing of a door's fades (#1405, #1512), as the next room's loading runs them.</summary>
public static class DoorTransitionColourDefinitions
{
    /// <summary>The length of a door's fades: the source room fades out, and the room it enters fades in, over this many steps.</summary>
    public const int FadeDenominator = 12;
}
