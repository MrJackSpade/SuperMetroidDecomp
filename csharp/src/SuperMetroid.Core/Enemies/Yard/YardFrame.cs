namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One Yard pose; its spritemap is confined to <see cref="Artwork.YardFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct YardFrame(int Pose);
