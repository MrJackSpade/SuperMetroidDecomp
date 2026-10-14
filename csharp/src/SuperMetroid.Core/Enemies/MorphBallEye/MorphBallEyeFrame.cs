namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>One Morph Ball eye pose; its spritemap is confined to <see cref="Artwork.MorphBallEyeFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MorphBallEyeFrame(int Pose);
