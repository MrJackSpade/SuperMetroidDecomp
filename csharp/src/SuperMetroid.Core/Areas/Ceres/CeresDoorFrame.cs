namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres door pose; its spritemap is confined to <see cref="Artwork.CeresDoorFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct CeresDoorFrame(int Pose);
