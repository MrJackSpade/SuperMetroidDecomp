namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Nuclear Waffle pose; its spritemap is confined to <see cref="Artwork.NuclearWaffleFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct NuclearWaffleFrame(int Pose);
