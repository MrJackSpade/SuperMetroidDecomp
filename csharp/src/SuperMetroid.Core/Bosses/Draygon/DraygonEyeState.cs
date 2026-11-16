namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The state of Draygon's eye actor.</summary>
internal sealed class DraygonEyeState
{
    /// <summary>Starts the eye of <paramref name="draygon"/> in <paramref name="action"/>.</summary>
    internal DraygonEyeState(DraygonEnemyState draygon, DraygonEyeAction action)
    {
        Draygon = Ensure.NotNull(draygon);
        Action = action;
    }

    /// <summary>The encounter the eye is part of (#2177), which his body set up before it.</summary>
    internal DraygonEnemyState Draygon { get; }

    /// <summary>The eye's action.</summary>
    public DraygonEyeAction Action { get; internal set; }

    /// <summary>The way the tracking eye faces, Draygon's (#2165).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>The bearing toward Samus the eye last looked along.</summary>
    public Bearing LookAngle { get; internal set; }
}
