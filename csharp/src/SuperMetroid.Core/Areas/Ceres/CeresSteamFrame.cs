namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres steam pose; its spritemap is confined to <see cref="Artwork.CeresSteamFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct CeresSteamFrame(int Pose);
