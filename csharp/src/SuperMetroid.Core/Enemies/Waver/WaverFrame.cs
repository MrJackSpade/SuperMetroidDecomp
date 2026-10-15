namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One Waver pose; its spritemap is confined to <see cref="Artwork.WaverFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct WaverFrame(int Pose);
