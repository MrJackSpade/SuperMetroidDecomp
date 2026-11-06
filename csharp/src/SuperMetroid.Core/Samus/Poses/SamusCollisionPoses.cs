using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Forms what room collision needs to know about a pose (#1395).</summary>
internal static class SamusCollisionPoses
{
    /// <summary>What room collision needs to know about <paramref name="pose"/>.</summary>
    internal static SamusCollisionPose ForRoomCollision(this SamusPoseId pose) =>
        pose is SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose ? SamusCollisionPose.StandingNormal
        : pose == SamusPoseId.RanIntoWallLeftPose ? SamusCollisionPose.RanIntoWallLeft
        : pose == SamusPoseId.RanIntoWallRightPose ? SamusCollisionPose.RanIntoWallRight
        : SamusPoseMetadata.ArmsElevator(pose) ? SamusCollisionPose.StandingOther
        : SamusCollisionPose.Other;
}
