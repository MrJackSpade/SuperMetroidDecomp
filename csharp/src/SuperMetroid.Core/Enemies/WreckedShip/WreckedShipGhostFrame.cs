namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One Wrecked Ship ghost pose; its spritemap is confined to <see cref="Artwork.WreckedShipGhostFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct WreckedShipGhostFrame(int Pose);
