namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One Wall Space Pirate pose; its spritemap is confined to <see cref="Artwork.WallSpacePirateFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct WallSpacePirateFrame(int Pose);
