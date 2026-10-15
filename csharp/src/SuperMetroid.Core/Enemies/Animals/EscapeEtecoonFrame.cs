namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon pose; its spritemap is confined to <see cref="Artwork.EscapeEtecoonFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct EscapeEtecoonFrame(int Pose);
