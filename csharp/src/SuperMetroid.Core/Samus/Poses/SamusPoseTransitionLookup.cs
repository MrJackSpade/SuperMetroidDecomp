namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What the input-transition lookup decides: a new pose, the pose's no-input fallback, or keeping the pose.
/// </summary>
public abstract record SamusPoseTransitionLookup
{
    /// <summary>Only the three outcomes derive from it (#2560).</summary>
    private SamusPoseTransitionLookup()
    {
    }

    /// <summary>No transition: the pose is kept, and its definition's fallback is not consulted.</summary>
    public static SamusPoseTransitionLookup KeepsPose { get; } = new Keeping();

    /// <summary>No transition: the pose's definition decides, through its no-input fallback.</summary>
    public static SamusPoseTransitionLookup PoseDefinitionFallback { get; } = new FallingBack();

    /// <summary>The transition the lookup chose, or null when it chose none.</summary>
    public SamusPoseTransition? Transition => this is Chosen chosen ? chosen.Next : null;

    /// <summary>Whether the lookup leaves the choice to the pose definition's fallback.</summary>
    public bool UsesPoseDefinitionFallback => this is FallingBack;

    /// <summary>A transition to a new pose.</summary>
    /// <param name="Next">The transition chosen.</param>
    public sealed record Chosen(SamusPoseTransition Next) : SamusPoseTransitionLookup;

    /// <summary>The pose is kept.</summary>
    private sealed record Keeping : SamusPoseTransitionLookup;

    /// <summary>The pose definition's fallback decides.</summary>
    private sealed record FallingBack : SamusPoseTransitionLookup;
}
