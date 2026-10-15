namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>One Sbug pose; its spritemap is confined to <see cref="Artwork.SbugFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct SbugFrame(int Pose);
