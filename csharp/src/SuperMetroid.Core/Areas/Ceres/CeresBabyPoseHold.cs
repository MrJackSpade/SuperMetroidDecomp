
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A baby metroid pose and the number of frames it is held.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Duration">The duration.</param>
internal readonly record struct CeresBabyPoseHold(CeresBabyPose Pose, int Duration);
