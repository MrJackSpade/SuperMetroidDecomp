namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One Ninja Space Pirate pose; its spritemap is confined to <see cref="Artwork.NinjaSpacePirateFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct NinjaSpacePirateFrame(int Pose);
