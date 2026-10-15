namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One Stoke pose; its spritemap is confined to <see cref="Artwork.StokeFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct StokeFrame(int Pose);
