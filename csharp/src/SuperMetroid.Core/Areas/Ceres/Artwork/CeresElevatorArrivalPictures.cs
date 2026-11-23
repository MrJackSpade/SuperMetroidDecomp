namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The Ceres elevator arrival's pictures (#2425): the pad's two poses and the platform's one, each on the room's shared
/// spawned-object sheet under its own name, which the import installs from the pictures' compositions.
/// </summary>
internal static partial class CeresElevatorArrivalPictures
{
    /// <summary>The name the spawned-object sheet holds the picture of <paramref name="pose"/> under.</summary>
    internal static string SheetName(CeresElevatorArrivalPose pose) =>
        $"CeresElevatorArrival.{CeresElevatorArrivalPoseKeys.Installed.Of(pose)}";
}
