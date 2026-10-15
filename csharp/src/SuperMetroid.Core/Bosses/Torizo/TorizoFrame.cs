namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Torizo body pose; its spritemap is confined to <see cref="Artwork.TorizoFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct TorizoFrame(int Pose);
