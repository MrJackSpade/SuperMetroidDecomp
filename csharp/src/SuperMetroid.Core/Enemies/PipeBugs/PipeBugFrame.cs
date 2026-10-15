namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>One Pipe Bug pose; its spritemap is confined to <see cref="Artwork.PipeBugFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct PipeBugFrame(int Pose);
