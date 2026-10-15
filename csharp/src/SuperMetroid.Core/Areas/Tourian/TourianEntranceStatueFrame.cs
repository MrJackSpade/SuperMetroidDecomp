namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian entrance statue pose; its spritemap is confined to <see cref="Artwork.TourianEntranceStatueFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct TourianEntranceStatueFrame(int Pose);
