namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts only pixels whose winning OAM record has this priority.</summary>
/// <param name="Priority">The OBJ priority tier, 0 through <see cref="MaximumPriority"/>.</param>
/// <param name="FixedColor">Optional fixed color added to the inserted pixels.</param>
public sealed record ObjPriorityRenderLayer(int Priority, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer
{
    /// <summary>The highest of the four OBJ priority tiers.</summary>
    public const int MaximumPriority = 3;

    /// <summary>The OBJ priority tier.</summary>
    public int Priority
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumPriority, value);
    } = Ensure.BetweenInclusive(0, MaximumPriority, Priority);
}
