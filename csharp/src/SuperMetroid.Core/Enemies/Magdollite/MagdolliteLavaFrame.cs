namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One Magdollite lava pose; its installed artwork key is confined to <see cref="Artwork.MagdolliteLavaFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct MagdolliteLavaFrame(int Pose);
