namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>One elevator pose; its spritemap is confined to <see cref="Artwork.ElevatorFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ElevatorFrame(int Pose);
