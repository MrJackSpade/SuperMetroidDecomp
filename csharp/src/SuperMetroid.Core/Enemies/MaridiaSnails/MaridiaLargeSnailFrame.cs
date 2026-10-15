namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One Oum pose; its spritemap is confined to <see cref="Artwork.MaridiaLargeSnailFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MaridiaLargeSnailFrame(int Pose);
