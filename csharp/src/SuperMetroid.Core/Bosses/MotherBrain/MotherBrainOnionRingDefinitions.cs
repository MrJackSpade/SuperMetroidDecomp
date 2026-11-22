namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's onion rings (#2171): how long one waits at her mouth, how fast it flies and the arc it is aimed within.</summary>
internal static partial class MotherBrainOnionRingDefinitions
{
    /// <summary>
    /// The updates from an onion ring's first to its flight, the ring staying at Mother Brain's mouth on the eight before.
    /// </summary>
    internal const int MouthWaitUpdates = 9;

    /// <summary>The speed an onion ring flies at along its aim: four and five sixteenths pixels an update.</summary>
    internal static readonly SignedEightEight Speed = SignedEightEight.FromPixelFraction(69, 16);

    /// <summary>The height above which a flying onion ring bursts against the ceiling.</summary>
    internal const int ArenaTopY = 32;

    /// <summary>The height at or below which a flying onion ring bursts against the floor.</summary>
    internal const int ArenaFloorY = 216;

    /// <summary>How far right of the camera's left edge a flying onion ring bursts at the screen's edge.</summary>
    internal const int ScreenRightEdgeX = 248;

    /// <summary>The lowest aim an onion ring launches at: a sixteenth of a turn.</summary>
    internal static readonly TrigonometryAngle LowestAim = new(16);

    /// <summary>The highest aim an onion ring launches at: nine thirty-seconds of a turn.</summary>
    internal static readonly TrigonometryAngle HighestAim = new(72);

    /// <summary>Where the onion-ring aim clamp stops raising aims to the highest and starts lowering them to the lowest: three quarters of a turn.</summary>
    internal static readonly TrigonometryAngle ClampSplit = new(192);
}
