namespace SuperMetroid.Core.Samus;

/// <summary>
/// Where Ceres Ridley's shove of Samus stands (#2554): no shove, a shove whose first update sets her up, or a shove carrying
/// her toward the wall its first update chose.
/// </summary>
public abstract record CeresRidleyShoveStage
{
    /// <summary>Only the stages below are choices.</summary>
    private CeresRidleyShoveStage()
    {
    }

    /// <summary>No shove holds her movement.</summary>
    public sealed record Inactive : CeresRidleyShoveStage;

    /// <summary>The shove holds her movement and its first update has yet to install her pose and velocity.</summary>
    public sealed record Starting : CeresRidleyShoveStage;

    /// <summary>The shove carries her toward the wall <paramref name="Toward"/> until she meets it.</summary>
    /// <param name="Toward">The wall the shove carries her toward, chosen from her place on the screen.</param>
    public sealed record Pushing(HorizontalDirection Toward) : CeresRidleyShoveStage;
}
