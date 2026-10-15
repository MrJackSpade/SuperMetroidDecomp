namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One Magdollite pose; its spritemap is confined to <see cref="Artwork.MagdolliteFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MagdolliteFrame(int Pose);
