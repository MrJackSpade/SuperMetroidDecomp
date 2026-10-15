namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot laser pose; its installed artwork key is confined to <see cref="Artwork.WorkRobotLaserFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct WorkRobotLaserFrame(int Pose);
