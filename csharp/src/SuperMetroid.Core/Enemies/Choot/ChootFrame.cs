namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot pose; its spritemap is confined to <see cref="Artwork.ChootFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ChootFrame(int Pose);
