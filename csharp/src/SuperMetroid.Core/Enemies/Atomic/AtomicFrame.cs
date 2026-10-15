namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic pose; its spritemap is confined to <see cref="Artwork.AtomicFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct AtomicFrame(int Pose);
