namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>One Kzan pose; its spritemap is confined to <see cref="Artwork.KzanFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct KzanFrame(int Pose);
