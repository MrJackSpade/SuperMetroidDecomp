namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Tourian enemy pose; its spritemap is confined to <see cref="Artwork.DeadTourianFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct DeadTourianFrame(int Pose);
