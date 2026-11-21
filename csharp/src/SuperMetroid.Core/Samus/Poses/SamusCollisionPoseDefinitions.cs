using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The poses that present room collision a kind of their own (<see cref="SamusCollisionPoses"/>).</summary>
internal static partial class SamusCollisionPoseDefinitions
{
    /// <summary>
    /// Each pose that presents a collision kind of its own: neutral standing, facing either way, and running into a wall
    /// on either side. Every other pose presents standing when an elevator arms under it, and otherwise nothing a
    /// pose-gated reaction admits.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, SamusCollisionPose> Presented = new Dictionary<SamusPoseId, SamusCollisionPose>
    {
        [SamusPoseId.FacingRightNormalPose] = SamusCollisionPose.StandingNormal,
        [SamusPoseId.FacingLeftNormalPose] = SamusCollisionPose.StandingNormal,
        [SamusPoseId.RanIntoWallLeftPose] = SamusCollisionPose.RanIntoWallLeft,
        [SamusPoseId.RanIntoWallRightPose] = SamusCollisionPose.RanIntoWallRight,
    };
}
