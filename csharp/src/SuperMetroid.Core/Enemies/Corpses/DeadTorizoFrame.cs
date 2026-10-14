namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Dead Torizo pose; its spritemap is confined to <see cref="Artwork.DeadTorizoFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct DeadTorizoFrame(int Pose);
