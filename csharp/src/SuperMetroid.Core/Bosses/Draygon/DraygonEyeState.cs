namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The state of Draygon's eye actor.</summary>
internal sealed class DraygonEyeState
{
    /// <summary>The eye's function.</summary>
    public DraygonEyeFunction? Function { get; internal set; }

    /// <summary>The bearing toward Samus the eye last looked along, 0 through 255.</summary>
    public int LookAngle
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }
}
