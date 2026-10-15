namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot pose; its spritemap is confined to <see cref="Artwork.WorkRobotFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct WorkRobotFrame(int Pose);
