namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hopper pose; its spritemap is confined to <see cref="Artwork.HopperFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct HopperFrame(int Pose);
