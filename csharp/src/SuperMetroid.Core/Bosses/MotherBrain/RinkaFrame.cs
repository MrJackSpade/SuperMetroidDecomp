namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Rinka pose; its spritemap is confined to <see cref="Artwork.RinkaFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct RinkaFrame(int Pose);
