namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>One Bull pose; its spritemap is confined to <see cref="Artwork.BullFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct BullFrame(int Pose);
