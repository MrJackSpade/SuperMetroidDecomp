namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The state of Draygon's eye actor.</summary>
internal sealed class DraygonEyeState
{
    /// <summary>The eye's action.</summary>
    public DraygonEyeAction? Action { get; internal set; }

    /// <summary>The bearing toward Samus the eye last looked along.</summary>
    public Bearing LookAngle { get; internal set; }
}
