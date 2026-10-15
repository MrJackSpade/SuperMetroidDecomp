namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One Beetom pose; its spritemap is confined to <see cref="Artwork.BeetomFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct BeetomFrame(int Pose);
