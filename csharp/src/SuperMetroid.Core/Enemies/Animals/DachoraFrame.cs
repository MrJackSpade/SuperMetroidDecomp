namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Dachora pose; its spritemap is confined to <see cref="Artwork.DachoraFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct DachoraFrame(int Pose);
