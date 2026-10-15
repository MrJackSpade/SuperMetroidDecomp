namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One shutter pose; its spritemap is confined to <see cref="Artwork.ShutterFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ShutterFrame(int Pose);
