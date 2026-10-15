namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree or Metaree pose; its spritemap is confined to <see cref="Artwork.SkreeMetareeFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct SkreeMetareeFrame(int Pose);
