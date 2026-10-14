namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One gunship pose; its spritemap is confined to <see cref="Artwork.GunshipFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct GunshipFrame(int Pose);
