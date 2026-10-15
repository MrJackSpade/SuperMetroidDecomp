namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One Walking Space Pirate pose; its spritemap is confined to <see cref="Artwork.WalkingSpacePirateFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct WalkingSpacePirateFrame(int Pose);
