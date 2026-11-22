using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The poses a Grapple drop leaves Samus in apart from her shot direction and body radius.</summary>
internal static partial class SamusGrappleDropDefinitions
{
    /// <summary>
    /// The swinging poses, each with the ordinary standing pose it drops to: they ignore the shot direction and always fall
    /// to ordinary standing, even though their live collision radius is compact.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, SamusPoseId> StandingDrops = new Dictionary<SamusPoseId, SamusPoseId>
    {
        [SamusPoseId.GrappleSwingRightPose] = SamusPoseId.FacingRightNormalPose,
        [SamusPoseId.GrappleSwingLeftPose] = SamusPoseId.FacingLeftNormalPose,
    };
}
