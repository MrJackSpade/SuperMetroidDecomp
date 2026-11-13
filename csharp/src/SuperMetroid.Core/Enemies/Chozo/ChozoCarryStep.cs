namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One step of a Chozo statue's carry: the way the statue faces and the pose it carries Samus in.</summary>
/// <param name="Facing">The way the statue faces and walks: the Wrecked Ship statue faces left, Lower Norfair's right.</param>
/// <param name="Pose">The pose the statue carries Samus in.</param>
public readonly record struct ChozoCarryStep(HorizontalDirection Facing, ChozoCarryPose Pose);
