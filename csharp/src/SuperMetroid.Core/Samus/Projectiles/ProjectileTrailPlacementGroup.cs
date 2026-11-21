using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One placement a trail list gives, stated once, and the frames it gives it for (#2557).</summary>
/// <param name="Placement">Where the trail's two sparkles start.</param>
/// <param name="Frames">The frames of a shot the list gives the placement for.</param>
internal sealed partial record ProjectileTrailPlacementGroup(ProjectileTrailPlacement Placement, SamusProjectileFrameId[] Frames)
{
    /// <summary>A list's placement for each frame its groups name.</summary>
    /// <param name="groups">The list's placements, each with its frames.</param>
    /// <exception cref="ArgumentException">Two groups name the same frame.</exception>
    internal static FrozenDictionary<SamusProjectileFrameId, ProjectileTrailPlacement> ByFrame(ProjectileTrailPlacementGroup[] groups)
    {
        var byFrame = new Dictionary<SamusProjectileFrameId, ProjectileTrailPlacement>();
        foreach (ProjectileTrailPlacementGroup group in Ensure.NotNull(groups))
        foreach (SamusProjectileFrameId frame in group.Frames)
            byFrame.Add(frame, group.Placement);
        return byFrame.ToFrozenDictionary();
    }
}
