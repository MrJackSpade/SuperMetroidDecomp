namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One installed animation frame of a pose's body (#1517): the half pictures it selects, always a top picture and a
/// bottom picture unless the frame keeps showing the bottom picture an earlier frame selected, and where the frame places
/// the body. The import bakes each frame's placement, so drawing places the body by pose and frame alone.
/// </summary>
/// <param name="Top">The top half's picture.</param>
/// <param name="Bottom">The bottom half's picture, or null for a frame that keeps the bottom already shown.</param>
/// <param name="OriginRise">How many pixels above Samus's drawn point the frame's body origin sits; negative below it.</param>
public readonly record struct SamusBodyFramePictures(SamusBodyPicture Top, SamusBodyPicture? Bottom, int OriginRise);
