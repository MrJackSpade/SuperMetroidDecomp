namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One Norfair lava jumper pose; its spritemap is confined to <see cref="Artwork.NorfairLavaJumperFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct NorfairLavaJumperFrame(int Pose);
