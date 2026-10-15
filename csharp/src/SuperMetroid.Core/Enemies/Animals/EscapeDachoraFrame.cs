namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Dachora pose; its spritemap is confined to <see cref="Artwork.EscapeDachoraFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct EscapeDachoraFrame(int Pose);
