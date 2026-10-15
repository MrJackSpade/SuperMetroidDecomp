namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One Owtch pose; its spritemap is confined to <see cref="Artwork.OwtchFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct OwtchFrame(int Pose);
