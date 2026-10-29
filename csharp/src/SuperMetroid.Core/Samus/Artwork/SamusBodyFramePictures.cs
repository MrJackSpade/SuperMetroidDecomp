namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The body half pictures one animation frame selects: always a top picture, and a bottom picture unless the frame keeps
/// showing the bottom picture an earlier frame selected.
/// </summary>
/// <param name="Top">The top half's picture.</param>
/// <param name="Bottom">The bottom half's picture, or null for a frame that keeps the bottom already shown.</param>
public readonly record struct SamusBodyFramePictures(SamusBodyPicture Top, SamusBodyPicture? Bottom);
