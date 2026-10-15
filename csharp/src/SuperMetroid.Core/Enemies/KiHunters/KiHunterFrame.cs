namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One Ki-Hunter pose; its spritemap is confined to <see cref="Artwork.KiHunterFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct KiHunterFrame(int Pose);
