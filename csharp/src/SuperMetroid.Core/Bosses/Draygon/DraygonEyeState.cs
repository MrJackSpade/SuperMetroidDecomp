namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The tracking of Draygon's eye, which his encounter keeps (#2298).</summary>
internal sealed class DraygonEyeState
{
    /// <summary>
    /// The way the eye faces while it tracks Samus, Draygon's (#2165); null while it is inert, which faces no way (#2560).
    /// </summary>
    public HorizontalDirection? TrackingFacing { get; internal set; }

    /// <summary>The direction toward Samus the eye last looked along: straight up before its first look.</summary>
    public TrigonometryAngle LookAngle { get; internal set; } = TrigonometryAngle.QuarterTurn;
}
