namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One Dragon pose; its spritemap is confined to <see cref="Artwork.DragonFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct DragonFrame(int Pose);
