namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The tracking of Draygon's eye, which his encounter keeps (#2298).</summary>
internal sealed class DraygonEyeState
{
    /// <summary>Starts the eye in <paramref name="action"/>.</summary>
    internal DraygonEyeState(DraygonEyeAction action) => Action = action;

    /// <summary>The eye's action.</summary>
    public DraygonEyeAction Action { get; internal set; }

    /// <summary>The way the tracking eye faces, Draygon's (#2165).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>The bearing toward Samus the eye last looked along.</summary>
    public Bearing LookAngle { get; internal set; }
}
