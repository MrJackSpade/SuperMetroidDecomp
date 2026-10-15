namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One Yapping Maw pose; its spritemap is confined to <see cref="Artwork.YappingMawFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct YappingMawFrame(int Pose);
