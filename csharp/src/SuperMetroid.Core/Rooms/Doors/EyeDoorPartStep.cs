namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One step of an eye door's body's or bottom's authored sequence (#2481): the pose it draws and how many updates the pose
/// stays. A part does nothing as it draws; only the eye shoots, sweats and smokes.
/// </summary>
/// <param name="Pose">The pose the step draws.</param>
/// <param name="Updates">The updates the pose stays before the next step, or before the sequence ends.</param>
internal readonly record struct EyeDoorPartStep(EyeDoorPartPose Pose, int Updates);
