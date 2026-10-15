namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Etecoon pose; its spritemap is confined to <see cref="Artwork.EtecoonFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct EtecoonFrame(int Pose);
