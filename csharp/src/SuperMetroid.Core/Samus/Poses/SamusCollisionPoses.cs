using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Forms what room collision needs to know about a pose (#1395).</summary>
internal static partial class SamusCollisionPoses
{
    /// <summary>What room collision needs to know about <paramref name="pose"/>.</summary>
    internal static SamusCollisionPose ForRoomCollision(this SamusPoseId pose) =>
        SamusCollisionPoseDefinitions.Presented.TryGetValue(pose, out SamusCollisionPose presented) ? presented
        : SamusPoseMetadata.ArmsElevator(pose) ? SamusCollisionPose.StandingOther
        : SamusCollisionPose.Other;
}
