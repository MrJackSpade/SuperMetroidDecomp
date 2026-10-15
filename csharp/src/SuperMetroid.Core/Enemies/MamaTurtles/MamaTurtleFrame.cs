namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One Mama Turtle pose; its spritemap is confined to <see cref="Artwork.MamaTurtleFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MamaTurtleFrame(int Pose);
