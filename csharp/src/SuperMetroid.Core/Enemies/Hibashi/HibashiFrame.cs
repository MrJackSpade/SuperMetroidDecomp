namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One Hibashi pose; its spritemap is confined to <see cref="Artwork.HibashiFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct HibashiFrame(int Pose);
